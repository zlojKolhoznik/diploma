---
description: Review last N commits as aggregate PR-style notes
argument-hint: "[N]"
---

Run an aggregate code review over the last N commits (default N=1).

If N is missing, treat it as 1. If N is not a positive integer, print usage: `/code-review [N]`.

1) Collect context:
- `git log -n "${N}" --oneline`
- `git diff "HEAD~${N}..HEAD"`

2) Produce PR-style markdown:

## Summary

## Findings
### Correctness / Design
### Security
### Performance
### Tests
### DX (Developer Experience)

## Risks

## Suggested follow-ups

## Test plan

Do not push or modify any files.