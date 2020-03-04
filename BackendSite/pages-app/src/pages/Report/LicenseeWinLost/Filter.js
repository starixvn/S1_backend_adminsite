import React, {forwardRef} from "react";
import moment from 'moment';
import CalendarIcon from 'mdi-react/CalendarIcon';

if(process.env.NODE_ENV!=='production'){
    window.gCurrencyList = [2,4,13,15,51];
    window.i18n = {"lbl_Currency2":"MYR","lbl_Currency4":"THB","lbl_Currency13":"RMB","lbl_Currency15":"IDR","lbl_Currency51":"VND"};
}

const Panel = (props)=>{
    return(
        <div className="filter">
            <div className="row-flex">
                <DropDownBox 
                    columnTitle="Currency"
                    options={window.gCurrencyList.map(curr=>{return {displayText:window.i18n["lbl_Currency"+curr], value:curr}})}
                    hasAll={true}
                    selectedValue={0}
                    dataColumn="Currency"
                    onChange={props.onChange}
                />
                <DatePicker 
                    columnTitle="From Date"
                    ref={{
                        selectdate:props.refInfo.selectdate_start,
                        trigger:props.refInfo.f_trigger_Start
                    }}
                    defaultValue={props.filterInfo.StartDate != null ? moment.unix(props.filterInfo.StartDate).format('l') : moment().add(-10, 'days').format('l')}
                />
                <DatePicker 
                    columnTitle="To Date"
                    ref={{
                        selectdate:props.refInfo.selectdate_end,
                        trigger:props.refInfo.f_trigger_End
                    }}
                    defaultValue={props.filterInfo.EndDate != null ? moment.unix(props.filterInfo.EndDate).format('l') : moment().format('l')}
                />
                <div className="button-group">
                    <button className="btn btn-submit" type="submit" onClick={props.ClickSubmit}>Submit</button>
                </div>
            </div>
        </div>
    )
}

const DatePicker = forwardRef((props, ref)=>{
    return(
        <div className="input-item">
            <label className="label">{props.columnTitle}</label>
            <div className="date">
                <input ref={ref.selectdate} type="text" defaultValue={props.defaultValue} size="8" />
                <a className="icon-calendar" size={18} color="#0C83D9" ref={ref.trigger}>
                    <CalendarIcon size={18} color="#0C83D9" />
                </a>
            </div>
        </div>
    )
})

const DropDownBox = (props)=>{
    const onChange = (e)=>{
        if(props.onChange){
            props.onChange(e.target.value, props.dataColumn);
        }
    }
    let optionRows = props.options.map(item=>{
        return(
            <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>{item.displayText}</option>
        )
    });
    return(
        <div className="input-item">
            <label className="label">{props.columnTitle}</label>
            <div className="field">
            <select className="select-field" defaultValue={props.selectedValue} onChange={onChange}>
                { props.hasAll ? <option value="0">All</option> : null}
                {optionRows}
            </select>
            </div>
        </div>
    )
}

export default Panel;