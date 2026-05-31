import logging
import time
from cmreslogging.handlers import CMRESHandler

logger = logging.getLogger("test_logger")
logger.setLevel(logging.INFO)

es_handler = CMRESHandler(
    hosts=[{'host': 'elasticsearch', 'port': 9200}],
    auth_type=CMRESHandler.AuthType.NO_AUTH,
    es_index_name="python-test-logs",
    raise_on_indexing_exceptions=True
)
logger.addHandler(es_handler)

try:
    logger.info("Testing elasticsearch logging from python!")
    print("Log dispatched. Waiting for background thread to push...")
    time.sleep(5)
    print("Done waiting.")
except Exception as e:
    print(f"Exception caught: {e}")
