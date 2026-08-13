# Sale transaction

Checkout uses serializable isolation. It reloads active variants, combines duplicate cart rows, validates available quantities, creates the sale and line items, reduces stock, and records negative stock movements before committing once.
