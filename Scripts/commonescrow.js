$(function () {
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
});