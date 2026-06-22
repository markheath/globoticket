---
name: in-depth-code-review
description: Multi-faceted code review of the current branch using sub-agents. Use when asked to review changes, a diff, or a PR for correctness, security, maintainability, etc.
---
Perform a thorough code review of the changes on the current branch, and use sub-agents to perform separate checks for each of the following areas:

1. Correctness
Has the feature been implemented correctly and is free from errors?

2. Completeness
Have all the requirements been fully addressed, including all items listed in the planning document?

3. Security
Is the code secure? Are inputs validated? Is authorization and authentication fully implemented for all endpoints?

4. Maintainability
Is the code clean, and well structured? Has any unnecessary complexity or duplication been introduced? Have we increased technical debt and are there any recommended refactorings that would leave the codebase in a better state?

5. Test Coverage
Have sufficient tests been introduced to cover the new functionality and protect against regressions? Are these tests run as part of the CI build?

6. Performance
Are there any performance regressions or concerns? Consider whether database queries are optimized and require only minimal data. Highlight any inefficient algorithms.

7. Resilience
Are transient errors handled in a recoverable way? Is the code idempotent so that retries are safe?

8. Observability
Is there sufficient logging and telemetry to be able to monitor the health of this feature in production, and to diagnose the root cause of any issues?

9. Operability
Is the code ready to be deployed to production, with automation of any database migrations, and are there risks of disruption to live systems during upgrade? Is there sufficient configurability and use of feature flags? Are there any backwards compatibility risks?

10. Coding Conventions
If this repo includes a coding-conventions.md file, then evaluate whether all of the guidelines in that document have been followed. Otherwise check that changes are consistent with existing conventions and best-practices for the tech stack.

Report back with a summary of the issues found, organized by severity (critical/major/minor/suggestion). For each issue cite the file and line.
