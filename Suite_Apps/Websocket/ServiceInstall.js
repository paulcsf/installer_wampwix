var Service = require('node-windows').Service;

// Create a new service object
var svc = new Service({
  name:'Suite Websocket Server',
  description: 'The nodejs.org example web server.',
  script: 'C:\\SuiteDir\\WebSocket\\socket.js',
  wait: 1,
  grow: 0,
  maxRestarts: 60
});

// Listen for the "install" event, which indicates the
// process is available as a service.
svc.on('install',function(){
  svc.start();
});

svc.install();