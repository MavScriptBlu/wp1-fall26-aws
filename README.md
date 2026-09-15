# Knowledge Hub Document API

A small ASP.NET Core Web API that takes a file upload, checks it against a
few rules, and saves it to an Amazon S3 bucket. Built for Web Programming 1,
Parts 2-4 (deployment is Part 5).

## How storage works

Uploads go through a storage interface (`IDocumentStore`) instead of the
API code touching AWS directly. That split is on purpose - it means the
web layer has no idea it's talking to S3, and the tests don't need a real
AWS account to run.

Underneath that, there's one more layer: `IS3ObjectClient`. The real
implementation (`AwsS3ObjectClient`) wraps the AWS SDK. The tests swap in a
fake version that just records what got sent to it. That's how the test
suite proves the app works without ever making a network call.

## Validation rules

Checked before anything touches S3:

| Rule | Limit |
|---|---|
| Max file size | 1,048,576 bytes (1 MiB) |
| Empty files | rejected |
| Allowed extensions | .txt, .pdf, .png, .jpg, .jpeg (not case sensitive) |
| Allowed content types | text/plain, application/pdf, image/png, image/jpeg |

If a file fails more than one rule, you get all the errors back, not just
the first one.

## Where files land (the key format)

```
documents/uploads/{yyyy}/{MM}/{dd}/{random-id}.{ext}
```

Example: `documents/uploads/2026/09/14/3f2a91c84d7b4e6f8a105c2d9b7e0431.pdf`

The original file name never becomes part of the key - it's stored as
metadata instead. Keeps things safe (no weird characters in object paths)
and stops two people who upload a file with the same name from colliding.

Four bits of metadata get attached to every object:

- `original-filename`
- `content-type`
- `uploaded-utc`
- `content-length`

## Config

The bucket name and friends live under an `S3Storage` section:

```json
{
  "S3Storage": {
    "BucketName": "",
    "KeyPrefix": "documents/uploads",
    "Region": "us-east-1",
    "MaxFileSizeBytes": 1048576
  }
}
```

Locally, the bucket name comes from .NET user secrets (never committed).
On the server it comes from an environment variable instead - same key,
different source. Code doesn't care where the value came from, it just
asks for `S3Storage:BucketName`.

No AWS keys are ever hardcoded anywhere in this project. The SDK figures
out who you are on its own (your SSO session locally, an instance role on
the server).

## Running it locally

```bash
dotnet user-secrets --project src/Wp1Fall26Aws.Api set "S3Storage:BucketName" "<your-bucket>"
dotnet run --project src/Wp1Fall26Aws.Api
```

Then in another terminal:

```bash
curl -F "file=@samples/synthetic-document.txt;type=text/plain" http://localhost:5000/documents
```

## Endpoints

- `POST /documents` - upload a file (form field named `file`)
- `GET /health` - basic health check, no AWS calls
- `GET /` - just returns 200, mainly for the load balancer health check

## Tests

```bash
dotnet test Wp1Fall26Aws.slnx
```

All of them run offline - no AWS account needed, no network calls. That's
intentional so anyone can clone this and run the test suite with zero setup.

## Troubleshooting

Ran into a bunch of this myself getting the app talking to S3 locally, so here's what actually broke and how I fixed it:

**"Unable to get IAM security credentials" / "Failed to resolve AWS credentials"**
No AWS login active. Run `aws sso login`, sign in, try again. 
Sessions expire after 4 hours.

**"No RegionEndpoint or ServiceURL configured"**
Region isn't set. Run `aws configure set region us-east-1`.

**"BucketName is a required property..."**
Bucket name isn't set locally. Set it with user secrets: `dotnet user-secrets set "S3Storage:BucketName" "your-bucket-name-here"`

**"Access Denied" from S3**
Usually means you're logged into the wrong AWS account/identity. Check with `aws sts get-caller-identity` and make sure the account number matches where your bucket actually lives.

**"Assembly AWSSDK.SSOOIDC/AWSSDK.SSO could not be found"**
Using an SSO login needs two extra  packages the base project doesn't ship with:
`dotnet add src/Wp1Fall26Aws.Api package AWSSDK.SSOOIDC`
`dotnet add src/Wp1Fall26Aws.Api package AWSSDK.SSO`


