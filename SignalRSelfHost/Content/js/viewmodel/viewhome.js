define(['jquery', 'knockout', 'signalr.hubs', 'text!../../../Views/home.html'],
    function (jquery, ko, signalr, templateHome) {

        // Viewmodel for home view
        return function () {
            var self = this;

            self.template = ko.observable(templateHome);

            self.items = ko.observableArray([
                { id: 1, name: "one" },
                { id: 2, name: "two" },
                { id: 3, name: "three" }
            ]);

            self.init = function () {
                // set up SignalR
                self.setupSignalr();
            }

            self.setupSignalr = function () {
                // set signalr base url
                jquery.connection.hub.url = "http://localhost:9000/signalr";

                // Declare a proxy to reference the hub.
                var chat = jquery.connection.mainHub;

                // Create a function that the hub can call to broadcast messages.
                chat.client.sendMessage = function (name, message) {
                    // Html encode display name and message.
                    var encodedName = $('<div />').text(name).html();
                    var encodedMsg = $('<div />').text(message).html();
                    // Add the message to the page.
                    jquery('#discussion').append('<li><strong>' + encodedName
                        + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
                };

                // Get the user name and store it to prepend to messages.
                //$('#displayname').val(prompt('Enter your name:', ''));
                jquery('#displayname').val("msk");


                // Set initial focus to message input box.
                jquery('#message').focus();

                // Start the connection.
                jquery.connection.hub.start().done(function () {
                    jquery('#sendmessage').click(function () {
                        // Call the Send method on the hub.
                        chat.server.send($('#displayname').val(), jquery('#message').val());

                        // Clear text box and reset focus for next comment.
                        jquery('#message').val('').focus();
                    });
                });
            };
        }
    });

