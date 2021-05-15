let commands = [{
    // required
    name: "pingpong",
    // required
    description: "pongs a ping",
    // required
    alias: "pp",
    // required
    permission: "User",
    // optional (defaults to false)
    targetRequired: false,
    // optional
    arguments: [{
        name: "times to ping",
        required: true
    }],
    // required
    execute: (gameEvent) => {
        // parse the first argument (number of times)
        let times = parseInt(gameEvent.Data);

        // we only want to allow ping pong up to 5 times
        if (times > 5 || times <= 0) {
            gameEvent.Origin.Tell("You can only ping pong between 1 and 5 times");
            return;
        }

        // we want to print out a pong message for the number of times they requested
        for (var i = 0; i < times; i++) {
            gameEvent.Origin.Tell(`^${i}pong #${i + 1}^7`);

            // don't want to wait if it's the last pong
            if (i < times - 1) {
                System.Threading.Tasks.Task.Delay(1000).Wait();
            }
        }
    }
}];

let plugin = {
    author: 'RaidMax',
    version: 1.1,
    name: 'Ping Pong Sample Command Plugin',

    onEventAsync: function (gameEvent, server) {
    },

    onLoadAsync: function (manager) {
        this.logger = _serviceResolver.ResolveService("ILogger");
        this.logger.WriteDebug("sample plugin loaded");

        const intArray = [
            1337,
            1505,
            999
        ];

        const stringArray = [
            "ping",
            "pong",
            "hello"
        ];

        _configHandler.SetValue("SampleIntegerValue", 123);
        _configHandler.SetValue("SampleStringValue", this.author);
        _configHandler.SetValue("SampleFloatValue", this.version);
        _configHandler.SetValue("SampleNumericalArray", intArray);
        _configHandler.SetValue("SampleStringArray", stringArray);

        this.logger.WriteDebug(_configHandler.GetValue("SampleIntegerValue"));
        this.logger.WriteDebug(_configHandler.GetValue("SampleStringValue"));
        this.logger.WriteDebug(_configHandler.GetValue("SampleFloatValue"));

        _configHandler.GetValue("SampleNumericalArray").forEach((element) => {
            this.logger.WriteDebug(element);
        });

        _configHandler.GetValue("SampleStringArray").forEach((element) => {
            this.logger.WriteDebug(element);
        });
    },

    onUnloadAsync: function () {
    },

    onTickAsync: function (server) {
    }
};