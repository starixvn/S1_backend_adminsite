import React, {Component} from 'react';
import { DateRange } from '@9softstudio/react-datepicker';
import moment from 'moment';
import DateFormat from '../module/Formatting/DateFormat';
import Lang from '../resource/Common/LangAPI';

const iconElement = (<i className="icon icon-calendar"></i>);
export class DateRangePicker extends Component {
    constructor(props) {
        super(props);
        this.now = new Date();
        const now = moment();

        this.dateInfo = {
            fromDate:now.clone().subtract(1,'months'),
            toDate: now.clone(),
            minDate:moment(now.clone().subtract(7,'years')).format('M/DD/YYYY'),
            maxDate:moment(now.clone().add(3,'years')).format('M/DD/YYYY')
            // minDate:'1/01/2009',
            // maxDate:'12/30/2030',
            // limitedMonthRange:1
        };
        
        this.state = {
            fromDate: this.dateInfo.fromDate,
            toDate: this.dateInfo.toDate,
            isEnableSingleDate: true
        }
    }
    handleChangeDateRange = (date = true) => {
        this.setState({fromDate:date.startDate,toDate:date.endDate});
    }
    handleToggle = () => {
        this.setState((prevState) => {
            return {
                isEnableSingleDate: !prevState.isEnableSingleDate
            }
        })
    }
    GetDateRange(){
        return {StartDate: new DateFormat(this.state.fromDate).format("yyyy/mm/dd") , EndDate: new DateFormat(this.state.toDate).format("yyyy/mm/dd")}
    }
    render() {
        const now = moment();
        const myRanges={ 
            [Lang.getLang(window.LangCode, "lblToday")]:{       
                startDate: now.clone(),
                endDate: now.clone()
            },
            [Lang.getLang(window.LangCode, "lblYesterday")]:{          
                startDate: now.clone().add(-1, 'days'),
                endDate: now.clone().add(-1, 'days')
            },
            [Lang.getLang(window.LangCode, "lblCurrentWeek")]:{          
                startDate: now.clone().startOf('week'),
                endDate: now.clone()
            },
            [Lang.getLang(window.LangCode, "lblCurrentMonth")]:{          
                startDate: now.clone().startOf('month'),
                endDate: now.clone()
            }
        };

        const dateInfo = this.dateInfo;
        const {fromDate, toDate} = this.state;
        const locale = {
            months: [Lang.getLang(window.LangCode, 'lblJan'), Lang.getLang(window.LangCode, 'lblFeb'), Lang.getLang(window.LangCode, 'lblMar'), 
            Lang.getLang(window.LangCode, 'lblApr'), 
            Lang.getLang(window.LangCode, 'lblMay'),
            Lang.getLang(window.LangCode, 'lblJun'),
            Lang.getLang(window.LangCode, 'lblJul'),
            Lang.getLang(window.LangCode, 'lblAug'),
            Lang.getLang(window.LangCode, 'lblSep'),
            Lang.getLang(window.LangCode, 'lblOct'),
            Lang.getLang(window.LangCode, 'lblNov'),
            Lang.getLang(window.LangCode, 'lblDec')],
            weekdays: [Lang.getLang(window.LangCode, "lblSunday"),
            Lang.getLang(window.LangCode, "lblMonday"),
            Lang.getLang(window.LangCode, "lblTuesday"),
            Lang.getLang(window.LangCode, "lblWednesday"),
            Lang.getLang(window.LangCode, "lblThursday"),
            Lang.getLang(window.LangCode, "lblFriday"),
            Lang.getLang(window.LangCode, "lblSaturday")],
            applyLabel:Lang.getLang(window.LangCode, "btnOK"),
            cancelLabel:Lang.getLang(window.LangCode, "btnCancel"),
        }

        return (    
            <DateRange startDate={fromDate} endDate={toDate}
                minDate={dateInfo.minDate} maxDate={dateInfo.maxDate}
                today={dateInfo.toDate}
                onChange={this.handleChangeDateRange}
                iconElement={iconElement}
                ranges={myRanges} 
                locale={locale}
            />
        )
    }
}