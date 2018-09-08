var LogEnable=false;		//false => most logging disabled, still enough to show it's running.
var EventLogger = require('node-windows').EventLogger;
var log = new EventLogger('Suite Websocket Server');

//http
var fs = require('fs');
var app = require('express')();
var http = require('http');
var server = http.createServer(app);


/* //https
var fs = require('fs');
var app = require('express')();
var https = require('https');
var server = https.createServer({
//    key: fs.readFileSync('../SSL/hosted.com-key.pem'),
//    cert: fs.readFileSync('../SSL/hosted.com-chain.pem') 
//    key: fs.readFileSync('../SSL/beta.hosted.com-key.pem'),
//    cert: fs.readFileSync('../SSL/beta.hosted.com-chain.pem') 
    key: fs.readFileSync('../../sources/devhosted/server.key'),
    cert: fs.readFileSync('../../sources/devhosted/server.crt') 
},app);
*/

var io = require('socket.io').listen(server);

/*io.sockets.on('connection',function (socket) {
    ...
});

app.get("/", function(request, response){
    ...
})*/
var Redis = require('ioredis');
var redis = new Redis('127.0.0.1','6880');
redis.on('message', function(channel, message) {
    console.log('Message Recieved: channel: ' + channel + '   message: ' + message);
    if(LogEnable) log.info('Message Recieved: channel: ' + channel + '   message: ' + message);
    message = JSON.parse(message);
    io.emit(channel + ':' + message.event, message.data);
});



redis.psubscribe('UpdateTowers', function(err, count) {
    console.log('psubscribe: UpdateTower: err: ' + err + '   count: ' + count);
    if(LogEnable) log.info('psubscribe: UpdateTowers: err: ' + err + '   count: ' + count);
});
redis.psubscribe('UpdateZones', function(err, count) {
    console.log('psubscribe: UpdateZones: err: ' + err + '   count: ' + count);
    if(LogEnable) log.info('psubscribe: UpdateZones: err: ' + err + '   count: ' + count);
});
redis.on('pmessage', function(subscribed, channel, iMsg) {
    JMsg = JSON.parse(iMsg);

    console.log('PMessage Recieved: subscribed: ' + subscribed + '   channel: ' + channel + '   iMsg: ' + iMsg);
    if(LogEnable) log.info('PMessage Recieved: subscribed: ' + subscribed + '   channel: ' + channel + '   iMsg: ' + iMsg);

    io.emit(channel, JMsg);
});


server.listen(20500, function(){
    console.log('Listening on Port 20500');
	log.info('Listening on Port 20500');
});
 
//Extra line is important, Wix installer loses last semi-colon otherwise.