# Error handling

Expected business failures produce clear messages near the active workflow. Unexpected server errors use the framework error boundary and production exception handler. Transaction failures must not be converted into apparent success messages.
