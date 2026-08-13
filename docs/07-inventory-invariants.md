# Inventory invariants

Quantity on hand must never become negative. Every change requires a nonzero amount and an audit note. Services update the current balance and insert the matching movement inside one database transaction so the two records cannot drift apart.
