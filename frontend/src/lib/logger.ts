import winston from 'winston';
import { ElasticsearchTransport } from 'winston-elasticsearch';

const esTransportOpts = {
  level: 'info',
  clientOpts: { node: process.env.ELASTICSEARCH_URL || 'http://elasticsearch:9200' },
  indexPrefix: 'nextjs-logs'
};

const esTransport = new ElasticsearchTransport(esTransportOpts);

const logger = winston.createLogger({
  level: 'info',
  format: winston.format.json(),
  transports: [
    new winston.transports.Console(),
    esTransport
  ]
});

export default logger;
