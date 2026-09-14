# Evidence Template

Fill this in as you go. Scrub anything with your AWS account number or any
key/token before this goes anywhere public - swap it for something like
`<account-id>` instead.

## Part 1 - IAM denial

- Principal:
- Action attempted:
- Resource:
- Result (explicit deny vs missing permission):
- Controlling policy:

## Part 2 - Storage boundary

- Object key from a test upload:
- Metadata attached to it:

## Part 3 - CI

- Passing run URL:
- Failing run URL (intentional break):
- Fixed/passing run URL:

## Part 4 - Operate and document

- CLI/logs evidence (ports, process, upload log line):
- Cleanup confirmation (test objects removed from bucket):

## Part 5 - Deployment

- Actions run URL for the deploy workflow:
- "Confirm identity" step output (the role, not your personal account):
- curl response from the deployed environment:
- Cleanup confirmation (`eb terminate` + `eb list` showing nothing left):
