# Sale cancellation

Completed sales are never deleted. An admin voids a sale with a reason, the sale becomes `Cancelled`, its quantities return to stock, and compensating `SaleVoided` movements document the reversal.
