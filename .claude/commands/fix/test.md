Fix the failing tests: $ARGUMENTS

1. Read the failing test(s) to understand what they assert
2. Determine whether the test is correct and the implementation is broken, or the test needs updating (implementation intentionally changed)
3. Fix the right thing — do not delete or weaken tests to make them pass

If the implementation is broken: trace back to root cause and fix in the correct CA layer.
If the test is outdated: update the test to reflect the new intended behavior, and document why.

Do not mock away the problem — if a test fails because of missing DB setup or wrong dependency, fix the setup.
