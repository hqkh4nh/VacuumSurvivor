# CLAUDE.md

<!--
Maintainer notes (stripped before this file enters Claude's context, so they cost no tokens):
- Behavioral guidelines only. Project facts (build commands, architecture, layout) go in a separate
  section or a separate CLAUDE.md — Claude can derive most of them from the codebase anyway.
- Keep this file under 200 lines. Longer files reduce adherence.
- Sections 7 and 8 are behavioral, not enforced. If a rule must never be violated, add a PreToolUse
  hook instead of trusting this file.
- Working if: fewer stray diff lines, fewer rewrites for overcomplication, fewer comments a
  new hire would have to ask about, questions arriving before implementation instead of after.
-->

Behavioral rules for writing code in this repo. Follow them unless a project-specific
instruction says otherwise.

**Scope:** For a single-file change under ~20 lines, skip the planning step in §1 and go
straight to work. Everything else applies always.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

- State your assumptions explicitly. If uncertain, ask before writing code.
- If multiple interpretations exist, present them — don't pick one silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

For multi-step work, state the plan first, one line per step with its check:

```text
1. [Step] → verify: [check]
2. [Step] → verify: [check]
```

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for scenarios that cannot occur.
- If you wrote 200 lines and it could be 50, rewrite it before showing it.

Test: would a senior engineer call this overcomplicated? If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor what isn't broken.
- Match existing style, even where you'd do it differently.
- Notice unrelated dead code? Mention it. Don't delete it.
- Remove imports, variables, and functions that *your* change orphaned. Leave pre-existing
  dead code alone unless asked.

Test: every changed line traces directly to the request.

## 4. Comments

**Write for a stranger reading this file in a year with no access to this conversation.**

Comment the *why*. The code already says the *what*.

Worth a comment:

- A non-obvious constraint or invariant — `// Auth middleware must run before rate limiting.`
- A workaround, with a link — `// Safari <17 fires this twice. https://bugs.webkit.org/...`
- A business rule the code can't reveal — `// Invoices unpaid past 30 days go to collections.`
- Why a slower or uglier approach was chosen — `// Sequential on purpose: the API caps at 5 rps.`

Never write:

- **Restatements of the code.** `// increment counter`, `// import React`, `// loop through users`
- **Conversation artifacts.** "as requested", "per your feedback", "new implementation",
  "updated logic", "fixed the bug", "this is the correct version now"
- **References to context outside the repo.** Task IDs, ticket numbers, PR numbers, plan step
  numbers, sprint names, dates, model or agent names. `// Implements task 5` is meaningless to
  everyone who wasn't in that thread. If a ticket reference genuinely matters, it belongs in the
  commit message, not the source.
- **History or diff narration.** "removed old parser", "previously used lodash",
  "replaces the v1 handler", and commented-out code. Git already stores this.
- **TODO / FIXME**, unless explicitly asked for.

Form:

- One line. Two only if the reason truly needs it. If it needs a paragraph, the code is the
  problem — simplify it, or write the explanation in `docs/` and link to it.
- A complete sentence, no filler. Cut "Note that…", "This function is responsible for…".
- Same language and style as the comments already in that file.
- Public APIs get the language's doc-comment format (JSDoc, docstring, godoc): what it does,
  the params and returns that aren't obvious, what it throws. Obvious private helpers get nothing.
- Don't add comments to code you didn't change.

| Don't | Do |
| --- | --- |
| `// Task 5: validate the email` | delete it — the function name says this |
| `// Changed this to fix the bug you mentioned` | delete it |
| `// Loop through users and send emails` | delete it |
| `// Retry 3 times` above `retry(3)` | `// Payment gateway drops ~1% of first attempts.` |
| `// TODO: maybe cache this later?` | delete it |

Test: would this comment confuse someone who never saw the chat, the ticket, or the plan?
Then delete it or rewrite it.

## 5. Verify, Don't Claim

**Turn the task into something checkable, then check it.**

- "Add validation" → write tests for the invalid inputs, then make them pass.
- "Fix the bug" → write a test that reproduces it, then make it pass.
- "Refactor X" → tests pass before and after.

And never fake a green result:

- No placeholder implementations that silently return dummy data.
- No swallowed exceptions to make an error disappear.
- Don't weaken, skip, or delete a test to make the suite pass. Report the failure instead.
- Don't invent APIs, flags, or config keys. Check the source or the docs first.

If you didn't run it, say so. "Should work" means "untested" — write that word.

## 6. Reporting

- Report in chat. Don't create `SUMMARY.md`, `CHANGES.md`, or any report file unless asked.
- Say what changed, what you verified, and what you couldn't verify. No victory laps.
- Shortcuts taken, walls hit, and anything left unfinished go first, not in a footnote.

## 7. Commit Messages

**Conventional, scopeless, no co-authors.**

- Use standard types (`feat:`, `fix:`, `chore:`, `refactor:`, …) and **omit the scope**.
- Correct: `feat: add user authentication`
- Wrong: `feat(auth): add user authentication`
- Never append `Co-authored-by` trailers or any AI co-authorship mention.

## 8. Staging

**Markdown and AI config files stay untracked. Ask first.**

- Never commit `.md` files — `CLAUDE.md` especially — without explicit permission.
  Exception: `README.md` is fine.
- Never commit the `.claude` or `.openspec` folders.
- Never run `git add .` or `git commit -a`. Stage files individually by path.
