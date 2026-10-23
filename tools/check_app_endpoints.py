#!/usr/bin/env python3
"""Check that the running local Vite app and .NET API implement the same contract.

Usage (from the repository root, with BOTH servers running):
    python3 tools/check_app_endpoints.py

Uses only Python's standard library. A good /content response alone is not proof
that the API binary implements the newer /values, /fireteams and /forces/check routes.
"""
import json
import os
import sys
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

API = os.environ.get("IMPERATOR_API", "http://localhost:5080").rstrip("/")
WEB = os.environ.get("IMPERATOR_WEB", "http://localhost:5173").rstrip("/")

PLAN = {
    "name": "API smoke test",
    "faction": "republic",
    "setting": "fireteam",
    "source": "custom",
    "sourceId": None,
    "tvLimit": 350,
    "modelLimit": 8,
    "fireteams": [],
    "characters": [],
    "soldiers": [],
}

ROUTES = [
    ("GET", "/content", lambda data: isinstance(data, dict) and "troops" in data),
    ("GET", "/values", lambda data: isinstance(data, list) and any(
        item.get("troop") == "rep_clone_trooper" and "tv" in item for item in data
    )),
    ("GET", "/fireteams", lambda data: isinstance(data, dict)
     and isinstance(data.get("fireteams"), list) and isinstance(data.get("pinning"), list)),
    ("POST", "/forces/check", lambda data: isinstance(data, dict)
     and "tv" in data and "models" in data and isinstance(data.get("findings"), list)),
]


def probe(base, method, path, shape):
    body = json.dumps(PLAN).encode() if method == "POST" else None
    req = Request(
        base + path, data=body, method=method,
        headers={"Content-Type": "application/json", "Accept": "application/json"},
    )
    try:
        with urlopen(req, timeout=10) as res:
            status = res.status
            raw = res.read()
            try:
                parsed = json.loads(raw)
            except ValueError:
                return f"HTTP {status}, but not JSON (possibly the web HTML fallback)"
            if not shape(parsed):
                return f"HTTP {status}, but unexpected JSON shape"
            return None
    except HTTPError as exc:
        return f"HTTP {exc.code}"
    except (URLError, TimeoutError, OSError) as exc:
        return f"connection failed ({getattr(exc, 'reason', str(exc))})"


def main():
    print(f"Checking .NET API: {API}")
    print(f"Checking Vite proxy: {WEB}\n")
    issues = {}
    for label, base in (("API", API), ("web", WEB)):
        for method, path, shape in ROUTES:
            failure = probe(base, method, path, shape)
            print(f"  {label:3s} {method:4s} {path:15s}: {failure or 'OK'}")
            if failure:
                issues[(label, method, path)] = failure

    if not issues:
        print("\nAll endpoints are working through both the API and the web app.")
        return 0

    print("\nDiagnosis:")
    if any(k[0] == "API" and k[2] != "/content" for k in issues):
        print("- The process answering at the API address does not provide the current API contract.")
        print("  On macOS, find the listening process: lsof -nP -iTCP:5080 -sTCP:LISTEN")
        print("  Stop that process, then launch the CURRENT code from the repository root:")
        print("    dotnet run --project app/src/Imperator.Api")
        print("  If IMPERATOR_API is set, check that it points at this server.")
    elif any(k[0] == "web" for k in issues):
        print("- The API is current, but the web server is not forwarding the same endpoints.")
        print("  Stop Vite and restart with: npm --prefix app/web run dev")
        print("  Check IMPERATOR_API and verify that your browser is on http://localhost:5173.")
    else:
        print("- Check that both servers are running and review the failures above.")
    print("  /content alone is insufficient: older APIs also serve content v0.5.0.")
    return 1


if __name__ == "__main__":
    sys.exit(main())
