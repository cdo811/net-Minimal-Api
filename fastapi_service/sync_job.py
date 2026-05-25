import os
import logging
from sqlalchemy import create_engine, text

logger = logging.getLogger("fastapi_app")

def sync_sql_server_to_postgres():
    logger.info("Starting sync job from SQL Server to PostgreSQL...")
    sql_server_uri = os.environ.get('SQL_SERVER_CONN', 'mssql+pymssql://sa:StrongPassword123!@db/master')
    postgres_uri = os.environ.get('POSTGRES_CONN', 'postgresql+psycopg2://postgres:postgres@postgres/postgres')

    try:
        sql_engine = create_engine(sql_server_uri)
        pg_engine = create_engine(postgres_uri)

        # 1. Fetch data from SQL Server
        with sql_engine.connect() as sql_conn:
            try:
                result = sql_conn.execute(text("SELECT * FROM Customers"))
                rows = result.fetchall()
                columns = list(result.keys())
            except Exception as e:
                logger.warning(f"Failed to read from Customers table in SQL Server. It might not exist yet. Error: {e}")
                return

        # 2. Write to PostgreSQL
        with pg_engine.connect() as pg_conn:
            # Ensure table exists
            pg_conn.execute(text("""
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID SERIAL PRIMARY KEY,
                    Gender VARCHAR(10),
                    Age INT,
                    AnnualIncome INT,
                    SpendingScore INT,
                    Profession VARCHAR(50),
                    WorkExperience INT,
                    FamilySize INT
                )
            """))

            # Truncate existing data
            pg_conn.execute(text("TRUNCATE TABLE Customers"))

            # Insert new data
            if rows:
                insert_query = text("""
                    INSERT INTO Customers (Gender, Age, AnnualIncome, SpendingScore, Profession, WorkExperience, FamilySize)
                    VALUES (:Gender, :Age, :AnnualIncome, :SpendingScore, :Profession, :WorkExperience, :FamilySize)
                """)
                
                data = [dict(zip(columns, row)) for row in rows]
                
                with pg_conn.begin():
                    for item in data:
                        pg_conn.execute(insert_query, {
                            "Gender": item.get("Gender", ""),
                            "Age": item.get("Age", 0),
                            "AnnualIncome": item.get("AnnualIncome", 0),
                            "SpendingScore": item.get("SpendingScore", 0),
                            "Profession": item.get("Profession", ""),
                            "WorkExperience": item.get("WorkExperience", 0),
                            "FamilySize": item.get("FamilySize", 0)
                        })

        logger.info(f"Successfully synced {len(rows)} records from SQL Server to PostgreSQL.")

    except Exception as e:
        logger.error(f"Error during sync job: {e}")
