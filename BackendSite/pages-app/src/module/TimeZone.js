import DateFormat from './Formatting/DateFormat';
import moment from 'moment';

export default class TimeZone{
    static UserTimeZone(){
        return new Date().getTimezoneOffset() / -60;
    }
    
    static toServerTimeZone(time, IsStartDate) {
        let TimeZone_Offset = this.UserTimeZone();
        let server_time = new Date(time);
        server_time.setHours( server_time.getHours() -4-TimeZone_Offset );
        
        let date = new moment(server_time);
        if(!IsStartDate){
            date.add(24, 'hours').subtract(1, 'seconds')
        }
        
        return new DateFormat(date).format("yyyy/mm/dd HH:MM:ss");
    }

    static toLocalTimeZone(time, ServerTimeZone){
        let server_time = new Date(time);
        let date = new moment(server_time);
        return date.add(this.UserTimeZone()-ServerTimeZone, 'hours')
    }
}