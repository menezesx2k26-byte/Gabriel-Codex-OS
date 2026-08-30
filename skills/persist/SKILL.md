---
name: persist
description: Use when the user sends /persist, starts a message with /persist, or asks to activate the persistent conversation controller.
---

# Persist

`/persist` is the short invocation alias for the sibling `persistent-conversation-controller` skill.

Immediately load and apply `../persistent-conversation-controller/SKILL.md` to the current workflow. Do not implement a second persistence mechanism and do not reinterpret the alias as a status request.

Invocation carries exactly the same authority and safety boundaries as `persistent-conversation-controller`.

If that skill cannot be loaded, report the bootstrap failure explicitly; never claim persistence is active from this shim alone.