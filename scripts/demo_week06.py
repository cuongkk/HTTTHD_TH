"""Run the week 06 HTTP demo against the configured local QTKD API.

Creates a new set of demo rows on each run. Start the API and apply migrations first.
"""

import json
import sys
import urllib.error
import urllib.request
from uuid import uuid4


BASE = sys.argv[1].rstrip("/") if len(sys.argv) > 1 else "http://localhost:5095"


def call(method, path, payload=None):
    data = None if payload is None else json.dumps(payload).encode("utf-8")
    headers = {} if data is None else {"Content-Type": "application/json"}
    request = urllib.request.Request(BASE + path, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request) as response:
            return response.status, json.load(response)
    except urllib.error.HTTPError as error:
        body = error.read().decode("utf-8")
        return error.code, json.loads(body) if body else None


def expect(method, path, status, payload=None):
    actual_status, body = call(method, path, payload)
    assert actual_status == status, f"{method} {path}: expected {status}, got {actual_status}: {body}"
    details = []
    if isinstance(body, dict):
        details = [f"{key}={body[key]}" for key in ("id", "code", "status") if key in body]
    print(f"{method} {path} -> {actual_status}" + (f" ({', '.join(details)})" if details else ""))
    return body


suffix = uuid4().hex[:8].upper()
customer = expect("POST", "/api/customers", 201, {"name": f"Demo {suffix}"})
customer_id = customer["id"]
expect("GET", f"/api/customers/{customer_id}", 200)
expect("PUT", f"/api/customers/{customer_id}", 200,
       {"name": f"Demo {suffix}", "status": "Active"})
assert any(row["id"] == customer_id for row in expect("GET", "/api/customers", 200))

contract_data = {
    "contractNo": f"HD-DEMO-{suffix}",
    "customerId": customer_id,
    "startDate": "2026-11-06",
    "endDate": "2027-11-05",
}
contract = expect("POST", "/api/contracts", 201, contract_data)
contract_id = contract["id"]
assert contract["status"] == "Draft"
error = expect("POST", f"/api/contracts/{contract_id}/submit", 400)
assert error["code"] == "CTR-02"
assert expect("GET", f"/api/contracts/{contract_id}", 200)["status"] == "Draft"
assert expect("POST", "/api/contracts", 400,
              {**contract_data, "status": "Approved"})["code"] == "CTR-03"
assert expect("PUT", f"/api/contracts/{contract_id}", 400,
              {**contract_data, "status": "Approved"})["code"] == "CTR-03"

complete = {
    **contract_data,
    "summary": "Cung cap dich vu bao tri",
    "documentReference": f"DOC-{suffix}",
}
expect("PUT", f"/api/contracts/{contract_id}", 200, complete)
submitted = expect("POST", f"/api/contracts/{contract_id}/submit", 200)
assert submitted["status"] == "PendingApproval"
expect("POST", f"/api/contracts/{contract_id}/submit", 409)
expect("PUT", f"/api/contracts/{contract_id}", 409, complete)
assert expect("GET", f"/api/contracts/{contract_id}", 200)["status"] == "PendingApproval"

price_list = expect("POST", "/api/pricelists", 201,
                    {"name": f"Bang gia {suffix}", "effectiveDate": "2026-11-06"})
price_list_id = price_list["id"]
assert any(row["id"] == price_list_id for row in expect("GET", "/api/pricelists", 200))
assert expect("POST", f"/api/pricelists/{price_list_id}/items", 400,
              {"serviceCode": f"SVC-{suffix}", "unitPrice": 0})["code"] == "InvalidPriceItem"
expect("POST", f"/api/pricelists/{price_list_id}/items", 201,
       {"serviceCode": f"SVC-{suffix}", "unitPrice": 1250000})
assert len(expect("GET", f"/api/pricelists/{price_list_id}/items", 200)) == 1
assert len(expect("GET", f"/api/pricelists/{price_list_id}", 200)["items"]) == 1
print(f"PASS: customerId={customer_id}, contractId={contract_id}, priceListId={price_list_id}")
