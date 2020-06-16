//wwwroot/js/site.js

let state = {
    userName: '',
    howLong: null,
    input: {},
    button: {},
    div: {}
};

let connection = null;

state.input.userName = document.getElementById('userName');
state.input.howLong = document.getElementById('howLong');
state.button.startLongRunningCall = document.getElementById('btnStartLongRunningCall');
state.div.output = document.getElementById('output');

const refreshUserNameAndHowLong = () => {

    state.userName = state.input.userName.value;
    state.howLong = state.input.howLong.value;

    if (state.userName.length > 1 && parseInt(state.howLong) > 0) {
        state.button.startLongRunningCall.disabled = false;
    }
    else {
        state.button.startLongRunningCall.disabled = true;
    }
};

state.input.userName.addEventListener('keyup', refreshUserNameAndHowLong);
state.input.howLong.addEventListener('keyup', refreshUserNameAndHowLong);


const CallLongRunningOperation = () => {

    const url = `${apiBase}/StartLongRunningCall`;
    let data = { userName: state.userName, wait: parseInt(state.howLong) * 60000 };

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)

    }).then((res) => {
        if (res.ok) {
            UpdateOutput(`Long Running Operation Launched`);
            res.text().then((text) => {
                UpdateOutput(text);
            });
        }

    }).catch((err) => {
        UpdateOutput(`Something bad happened: ${err}`);
    });

};


const UpdateOutput = (msg) => {
    state.div.output.innerHTML += `<li>${msg}</li>`;
};

const connectSignalR = () => {

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiBase}/${state.userName}`)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on('SendUpdate', SendUpdate);

    connection.onclose(() => UpdateOutput('disconnected'));

    UpdateOutput('Connecting...');

    connection.start()
        .then(() => console.log)
        .then(() => {
            UpdateOutput('You are connected to SignalR');
            UpdateOutput('Calling Long Running Operation...');
            CallLongRunningOperation();
        })
        .catch((err) => UpdateOutput(err));

};

const SendUpdate = (status) => {
    console.log(status);
    UpdateOutput(status.Message);
};

state.button.startLongRunningCall.addEventListener('click', () => {
    connectSignalR();
});