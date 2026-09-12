# Persistflow Program Execution Index

**Spec:** `docs/superpowers/specs/2026-09-12-persistflow-multichannel-backup-failback-design.md`

Execute in this order:

1. `2026-09-12-persistflow-linux-multichannel-runtime.md`
2. `2026-09-12-persistflow-backup-restore.md`
3. `2026-09-12-persistflow-failback.md`

The runtime plan must reach its four-run/reboot gate before production backup scheduling. Backup/restore must reach both Linux and Windows restore-drill gates before failback implementation may perform a real authority transfer.

## Acceptance-Criteria Coverage

| Spec criterion | Plan/task |
|---|---|
| Linux without Windows-only local dependency | Runtime Tasks 1-3 |
| Four independent persisted runs | Runtime Tasks 4 and 6 |
| Bounded heavy-worker concurrency | Runtime Task 5 |
| Reboot recovery | Runtime Task 6 |
| Encrypted scheduled Drive backup | Backup Tasks 2, 3 and 5 |
| Upload alone never means healthy | Backup Task 2 |
| Disposable Linux restore | Backup Task 4 |
| Disposable Windows restore | Backup Task 6 |
| Relinquish before Windows activation | Failback Tasks 2 and 3 |
| No dual authority | Failback Tasks 1-6 |
| Git reconstruction / dirty-state handling | Backup Tasks 1 and 2 |
| No plaintext secrets in Git/Drive metadata | Backup Tasks 2 and 3 |
| Continue workflows after Azure deallocation | Failback Tasks 5 and 6 |

## Program Gates

- Do not deploy production `persistd` services to Azure before local tests are green.
- Do not mark a recovery point healthy without restore evidence.
- Do not run production failback before a no-authority Windows rehearsal passes.
- Do not deallocate Azure until Windows is ACTIVE and smoke-tested.
