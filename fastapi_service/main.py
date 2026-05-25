import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
from apscheduler.schedulers.background import BackgroundScheduler
from sync_job import sync_sql_server_to_postgres
from es_logger import ESHandler

logger = logging.getLogger("fastapi_app")
logger.setLevel(logging.INFO)
es_handler = ESHandler(host="elasticsearch", port=9200, index_prefix="python-logs")
logger.addHandler(es_handler)

scheduler = BackgroundScheduler()

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    logger.info("Starting up FastAPI application...")
    
    # Schedule the job to run every hour
    scheduler.add_job(sync_sql_server_to_postgres, 'cron', minute='*')
    scheduler.start()
    
    # Run an immediate sync for testing
    scheduler.add_job(sync_sql_server_to_postgres)
    
    yield
    # Shutdown
    logger.info("Shutting down FastAPI application...")
    scheduler.shutdown()

app = FastAPI(lifespan=lifespan)

@app.get("/")
def read_root():
    logger.info("Root endpoint was accessed in FastAPI")
    return {"Hello": "World from FastAPI"}
