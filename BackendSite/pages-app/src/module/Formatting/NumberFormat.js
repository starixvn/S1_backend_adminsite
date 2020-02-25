export default class NumberFormat{
    static format(number){
        return number == null ? null : number.toFixed(2).toString().replace(/(\d{1,3})(?=(\d{3})+\.)/g, "$1,");
    }
}