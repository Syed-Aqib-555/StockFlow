# Data model

A category groups products, a product owns many size/color variants, and a supplier can source many products. Sales own immutable line items. Every quantity change creates a stock transaction tied to the affected variant and, where available, the acting user.
