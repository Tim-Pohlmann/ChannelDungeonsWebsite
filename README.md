# Comparison branch

Orphan branch holding the workflow that deploys the migration comparison to
GitHub Pages under `/compare` (see `.github/workflows/deploy-compare.yml`).
The production site at the root is rebuilt from `main`, so the live site is
unaffected.

- `/compare/` — landing page (side-by-side or solo views)
- `/compare/original/` — pre-migration static site (`7b65e27`)
- `/compare/new/` — fresh migration (`blazor/03-deploy`)
- `/compare/other/` — other-branch migration attempt (`41d898d`)

A push to `main` redeploys production without `/compare`; re-run the
workflow to bring the comparison back. Delete this branch when done.
