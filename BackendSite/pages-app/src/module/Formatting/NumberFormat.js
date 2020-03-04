export function number_format(n) {
    n += "";
    var arr = n.split(".");
    var re = /(\d{1,3})(?=(\d{3})+$)/g;
    return arr[0].replace(re, "$1,") + (arr.length == 2 ? "." + arr[1] : "");
}

export function number_toFixed(n, point){
    return (Math.floor(n*100)/100).toFixed(point);
}