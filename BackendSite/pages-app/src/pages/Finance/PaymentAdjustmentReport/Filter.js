import React, {forwardRef} from "react";
import moment from 'moment';

if(process.env.NODE_ENV!=='production'){
    window.gCurrencyList = [2,4,13,15,51,61];
    window.i18n = {"lbl_Currency2":"MYR","lbl_Currency4":"THB","lbl_Currency13":"RMB","lbl_Currency15":"IDR","lbl_Currency51":"VND","lbl_Currency61":"INR"};
    window.gAdjustTypeList = {"1":"Debit","2":"Credit","3":"Increase Bonus","4":"Reduce Bonus","5":"3RD"};
}

const Panel = (props)=>{
    return(
        <div className="filter">
            <div className="row-flex">
                <DropDownBox 
                    columnTitle="Type"
                    options={Object.keys(window.gAdjustTypeList).map(key=>{return{value:key, displayText:window.gAdjustTypeList[key]}})}
                    hasAll={true}
                    selectedValue={0}
                    dataColumn="Type"
                    onChange={props.onChange}
                />
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
            </div>
            <div className="row-flex">
                <InputField
                    columnTitle="User"
                    defaultValue=""
                    dataColumn="User"
                    onChange={props.onChange}
                />
                <InputField
                    columnTitle="TransID"
                    defaultValue=""
                    dataColumn="TransId"
                    onChange={props.onChange}
                />
                <div className="button-group">
                    <button className="btn btn-search" type="submit" onClick={props.ClickSubmit}>Submit</button>
                </div>
            </div>
        </div>
    )
}

const InputField = (props)=>{
    const onChange = (e)=>{
        if(props.onChange){
            props.onChange(e.target.value, props.dataColumn);
        }
    }
    return(
        <div className="input-item">
            <span className="label">{props.columnTitle}</span>
            <div className="field">
                <input type="text" defaultValue={props.defaultValue} onChange={onChange}/>
            </div>
        </div>
    )
}

const DatePicker = forwardRef((props, ref)=>{
    return(
        <div className="input-item">
            <span className="label">{props.columnTitle}</span>
            <div className="date">
                <input ref={ref.selectdate} type="text" defaultValue={props.defaultValue} size="8" />
                <a className="icon-calendar" ref={ref.trigger}></a>
            </div>
        </div>
    )
});

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
            <span className="label">{props.columnTitle}</span>
            <div className="field">
            <select defaultValue={props.selectedValue} onChange={onChange}>
                { props.hasAll ? <option value="0">All</option> : null}
                {optionRows}
            </select>
            </div>
        </div>
    )
}

export default Panel;