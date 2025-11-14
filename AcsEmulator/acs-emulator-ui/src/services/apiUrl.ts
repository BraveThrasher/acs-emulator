const { protocol, hostname, port } = window.location;

const isDefaultPort = 
  (protocol === "https:" && port === "443") ||
  (protocol === "http:" && port === "80");

const endpointHost = !port || isDefaultPort ? hostname : `${hostname}:${port}`;

export const ApiUrl = `${protocol}//${endpointHost}`;