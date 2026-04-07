#!/usr/bin/env python3
"""
Hook: PostToolUse Write|Edit
Runs `dotnet build --no-restore -q` on the affected Verendar service
when a .cs file is written/edited. Injects build errors as additionalContext
so Claude sees them immediately without running a separate build command.
Silently exits (no output) when there are no errors.
"""
import sys
import json
import re
import subprocess

SERVICES = ("Identity", "Vehicle", "Media", "Notification", "Ai", "Garage", "Payment", "Location")
ROOT = "e:/Working/DreamLab/Verendar"


def main():
    try:
        data = json.load(sys.stdin)
    except Exception:
        sys.exit(0)

    file_path = data.get("tool_input", {}).get("file_path", "")
    if not file_path.endswith(".cs"):
        sys.exit(0)

    m = re.search(r"(" + "|".join(SERVICES) + r")", file_path)
    if not m:
        sys.exit(0)

    service = m.group(1)
    proj = f"{ROOT}/{service}/Verendar.{service}"

    result = subprocess.run(
        ["dotnet", "build", proj, "--no-restore", "-q"],
        capture_output=True,
        text=True,
        cwd=ROOT,
    )

    combined = result.stdout + result.stderr
    # Only real C# compiler errors (CS####), not MSBuild informational messages
    errors = [line for line in combined.splitlines() if re.search(r": error CS\d+:", line)]

    if errors:
        msg = "\n".join(errors[:3])
        out = {
            "hookSpecificOutput": {
                "hookEventName": "PostToolUse",
                "additionalContext": f"Build errors in {service} after edit:\n{msg}",
            }
        }
        print(json.dumps(out))


if __name__ == "__main__":
    main()
