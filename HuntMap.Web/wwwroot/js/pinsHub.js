window.pinsHub = (function(){
    let connection;
    async function register(dotnetRef){
        if (!window.signalR) {
            console.error("SignalR script not loaded");
            return;
        }
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/pinHub")
            .withAutomaticReconnect()
            .build();
        connection.on("PinCreated", (dto) => dotnetRef.invokeMethodAsync("OnPinCreated", dto));
        connection.on("PinUpdated", (dto) => dotnetRef.invokeMethodAsync("OnPinUpdated", dto));
        connection.on("PinDeleted", (id) => dotnetRef.invokeMethodAsync("OnPinDeleted", id));
        try {
            await connection.start();
            console.log("Pin hub connected");
        } catch (e) {
            console.error("Hub connect error", e);
        }
    }
    return { register };
})();