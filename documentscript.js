var socket;
var socketStatusSpan;
var msgsBlock;

document.addEventListener("DOMContentLoaded",
function(event){
    console.log("ready!");
    socketStatusSpan = document.getElementById('socket-status');
    msgsBlock = document.getElementById('msgs'); 
});

function initConnection(){
    if(socket){
        if(socket.readyState != WebSocket.CLOSED) {
            window.alert("Already Connected!");
        } else {
            socketStatusSpan.innerHTML = "Connecting"
            socket = new WebSocket('ws://localhost:5266/task-requester');
            setEventListener();
        }
    } else {
        socket = new WebSocket('ws://localhost:5266/task-requester');
        socketStatusSpan.innerHTML = "Connecting"
        setEventListener();
    }    
}

function disconnect() {
    if(socket && socket.readyState != WebSocket.CLOSED) {
        socketStatusSpan.innerHTML = "DISCONNECTING";
        socket.close(1000, 'User Manual Disconnection.');
    } else {
        window.alert("Not Connected!");
    }
}

function setEventListener() {
    // on errored connection
    socket.addEventListener("error",(e) => {
        socketStatusSpan.innerHTML = "ERROR";
        console.log("error occured", e);
    });

    // on open connection
    socket.addEventListener("open",(e) => {
        socketStatusSpan.innerHTML = "CONNECTED";
        console.log("connection opened");
    });

    // on msg received
    socket.addEventListener("message",(e)=>{
        const objStr = e.data;
        const message = JSON.parse(objStr).message;
        socketStatusSpan.innerHTML = "MESSAGE RECEIVED";
        msgsBlock.innerHTML += `<div> ${message} </div>`
        
        
        setTimeout(()=>{
            if(socket.readyState) {
                socketStatusSpan.innerHTML = "CONNECTED";
            } else {
                socketStatusSpan.innerHTML = "DISCONNECTED";
            }
        }, 10_000);
        console.log("message received");
        return false;
    });

    // on close connection
    socket.addEventListener("close",(e)=>{
        if(socketStatusSpan.innerHTML == 'ERROR')
            socketStatusSpan.innerHTML = "ERROR - CLOSED";
        else        
            socketStatusSpan.innerHTML = "DISCONNECTED";
        console.log("closed");
    });
}

