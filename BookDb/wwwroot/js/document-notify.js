'use strict';

var docConnection = null;

function startDocumentNotify(documentId, onPageChanged) {
    if (!documentId) return;

    docConnection = new signalR.HubConnectionBuilder()
        .withUrl('/notify')
        .withAutomaticReconnect()
        .build();

    docConnection.on('PageChanged', function (data) {
        try {
            var payload = data;
            if (onPageChanged) onPageChanged(payload);
        } catch (e) {
            console.error(e);
        }
    });

    docConnection.start().then(function () {
        // join the document group
        docConnection.invoke('JoinDocumentGroup', documentId).catch(function (err) {
            console.error(err.toString());
        });
    }).catch(function (err) {
        console.error(err.toString());
    });
}

function stopDocumentNotify(documentId) {
    if (!docConnection) return;
    if (documentId) {
        docConnection.invoke('LeaveDocumentGroup', documentId).catch(function (err) {
            console.error(err.toString());
        });
    }
    docConnection.stop();
    docConnection = null;
}
