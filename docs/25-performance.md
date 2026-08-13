# Performance

Read-heavy screens use no-tracking queries and project only required aggregates. List pages load bounded recent history where practical. Future large catalogs should add server-side pagination and indexed search rather than filtering an unlimited in-memory list.
