//string.startsWith()
if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        return this.slice(0, str.length) == str;
    };
}
//containsDqeError()
function containsDqeError(o) {
    if (o.messages == undefined) return false;
    if (o.messages.length == 0) return false;
    for (var i = 0; i < o.messages.length; i++) {
        if (o.messages[i].Severity == 3) return true;
    }
    return false;
}
//getDqeData()
function getDqeData(o) {
    return o.data == undefined ? o : o.data;
}