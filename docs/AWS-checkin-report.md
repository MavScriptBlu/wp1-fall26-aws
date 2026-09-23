# AWS Deployment In-Class Check-in Report
**Student:** [Blu / [studentID]]
**Date:** Wednesday, September 23, 2026
**Repository:** [https://github.com/MavScriptBlu/Wp1Fall26Aws]

---

## 1. Milestone Audit
- [x] Completed - **GitHub Repository Creation**: Initialized repo, added .gitignore, set main branch.
- [x] Completed - **API Construction**: Built and verified the Knowledge Hub Document API locally based on the class guide markdown.
- [x] Completed - **CI/CD Pipeline YAML**: Created `.github/workflows/deploy.yml` for automated AWS deployment via OIDC (no stored access keys — the workflow assumes an IAM role directly).
- [x] Completed - **Troubleshooting & Roadblocks**: Detailed audit documented below.

---

## 2. Pain Points & Failed Attempts

1. Issue: `Not authorized to perform sts:AssumeRoleWithWebIdentity` on the OIDC step.
   What I tried that did NOT work: Rechecked the role ARN in the repo variable, confirmed it was under Variables (not Secrets), and reviewed the trust policy — all matched the standard branch-ref pattern and still failed.
   What actually fixed it: The role's trust policy was scoped to a **GitHub Environment**, not a branch ref (`repo:you/repo:environment:name` instead of `repo:you/repo:ref:refs/heads/main`). Added `environment: wp1-fall26-aws-env` to the job in the workflow YAML to match.

2. Issue: Deploy succeeded but the app crashed on the instance with a `.runtimeconfig.json` error.
   What I tried that did NOT work: Re-running `eb deploy` as-is, assuming it was a transient failure.
   What actually fixed it: `eb deploy` zips raw source by default, and .NET doesn't build from source on Elastic Beanstalk. Had to `dotnet publish` first, zip that output, and point `.elasticbeanstalk/config.yml`'s `deploy: artifact:` at the published zip.

3. Issue: Two small CLI typos (`aws elasticbeastalk` instead of `elasticbeanstalk`, `--versiion-label` instead of `--version-label`) caused a later step to fail after credentials had already succeeded — looked like a permissions issue at first.
   What I tried that did NOT work: Re-checking IAM permissions before actually reading the literal error text.

---

## 3. Current Status & Unresolved Questions
Currently stuck on: Nothing blocking. deployment is green and automated end-to-end via GitHub Actions.
Question for instructor: The official Part 5 materials only document the branch-ref-scoped trust policy shape. Worth adding the GitHub-Environment-scoped variant (and the `environment:` job key it requires) as a documented alternative, since at least one student's role was configured that way and it produces an identical error message to five other unrelated causes.
