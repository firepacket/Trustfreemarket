/// <reference path="jsencrypt.js" />
/// <reference path="triplesec-3.0.0-min.js" />


$(function () {

    $('#generate').click(function () {

        var keySize = 2048;
        crypt = new JSEncrypt({ default_key_size: keySize });

        var dt = new Date();
        var time = -(dt.getTime());
        crypt.getKey();
        dt = new Date();
        time += (dt.getTime());
        //$('#time-report').text('Generated in ' + time + ' ms');
        $('#privkey').text(crypt.getPrivateKey());
        $('#pubkey').text(crypt.getPublicKey());
    });



    $('#execute').click(function () {
        
        // Create the encryption object.
        var crypt = new JSEncrypt();

        // Set the private.
        crypt.setPrivateKey($('#privkey').text());
        //return;
        // If no public key is set then set it here...
        var pubkey = $('#pubkey').text();
        if (!pubkey) {
            $('#pubkey').text(crypt.getPublicKey());
        }
        
        // Get the input and crypted values.
        var input = $('#input').val();
        var crypted = $('#crypted').text();

        // Alternate the values.
        if (input) {
            $('#crypted').text(crypt.encrypt(input));
            $('#input').val('');
        }
        else if (crypted) {
            var decrypted = crypt.decrypt(crypted);
            
            $('#input').val(decrypted);
            $('#crypted').text('');
            
        }
    });
    progress = [];
    reset_progress = function (msg) {
        progress = [];
        return $("#progress-summary").html(msg || '');
    };
    progress_hook = function (p) {
        var h, pr, _i, _len;
        if (progress.length && (progress[progress.length - 1].what === p.what)) {
            progress[progress.length - 1] = p;
        } else {
            progress.push(p);
        }
        h = "";
        for (_i = 0, _len = progress.length; _i < _len; _i++) {
            pr = progress[_i];
            h += "," + pr.what + " " + pr.i + "/" + pr.total + " &nbsp;";
        }
        return $("#progress-summary").html(h);
    };
    progress_hook = function (p) {
        var h, pr, _i, _len;
        if (progress.length && (progress[progress.length - 1].what === p.what)) {
            progress[progress.length - 1] = p;
        } else {
            progress.push(p);
        }
        h = "";
        for (_i = 0, _len = progress.length; _i < _len; _i++) {
            pr = progress[_i];
            h += " " + pr.what + " " + pr.i + "/" + pr.total + " &nbsp;";
        }
        return $("#progress-summary").html(h);
    };

    $('#tsenc').click(function () {
        reset_progress();
        triplesec.encrypt({

            data: new triplesec.Buffer($("#tsresult").val()),
            key: new triplesec.Buffer($("#tspwd").val()),
            progress_hook: progress_hook

        }, function (err, buff) {

            if (!err) {
                $("#tsresult").val(buff.toString('hex'));
            }
            else { alert(err); }

        });
    });

    $('#tsdec').click(function () {
        reset_progress();
        triplesec.decrypt({

            data: new triplesec.Buffer($("#tsresult").val(), "hex"),
            key: new triplesec.Buffer($("#tspwd").val()),
            progress_hook: progress_hook

        }, function (err, buff) {

            if (!err) {
                console.log(buff.toString());
            }
            $("#tsresult").val(buff.toString());
        });

        
    });


});