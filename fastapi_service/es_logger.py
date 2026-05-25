import logging
from datetime import datetime
from elasticsearch import Elasticsearch

class ESHandler(logging.Handler):
    def __init__(self, host, port, index_prefix="python-logs"):
        super().__init__()
        self.es = Elasticsearch(f"http://{host}:{port}")
        self.index_prefix = index_prefix

    def emit(self, record):
        try:
            doc = {
                "@timestamp": datetime.utcnow().isoformat() + "Z",
                "level": record.levelname,
                "logger": record.name,
                "message": record.getMessage(),
                "filename": record.filename,
                "funcName": record.funcName,
                "lineno": record.lineno,
            }
            index_name = f"{self.index_prefix}-{datetime.utcnow().strftime('%Y.%m.%d')}"
            self.es.index(index=index_name, document=doc)
        except Exception:
            pass # Fail silently so app doesn't crash if ES is unreachable
