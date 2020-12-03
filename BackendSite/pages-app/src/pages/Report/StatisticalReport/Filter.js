import React, {forwardRef} from "react";
import moment from 'moment';
import CalendarIcon from 'mdi-react/CalendarIcon';

if(process.env.NODE_ENV!=='production'){
//     window.gCurrencyList = [2, 4, 13, 15, 51, 96];
//     window.i18n = { "lbl_Currency2": "MYR", 
//                     "lbl_Currency4": "THB", 
//                     "lbl_Currency13": "RMB", 
//                     "lbl_Currency15": "IDR", 
//                     "lbl_Currency51": "VND",
//                     "lbl_Currency96": "USDT" };
 }

const Panel = (props)=>{
    
    let siteListOptions = "";
    if (props.SiteList) {
        siteListOptions = props.SiteList.map(i => {
            return {
                value: Number(i.siteId),
                displayText: i.siteName
            };
        });
    }

    return(
        <div className="filter">
            <div className="row-flex">
                {
                    (siteListOptions) ?
                        <CheckBoxElem
                            Title="Site"
                            Options={siteListOptions}
                            Selected={props.filterInfo.SiteIds}
                            OnChange={props.onChangeCheckBox}
                            OnChangeAll={props.onChangeAllCheckBox}
                            dataColumn="SiteIds"
                        />
                        : null
                }
                <button className="btn btn-search" type="submit" onClick={props.SubmitBtnDisabled ? null : props.ClickSubmit} disabled={props.SubmitBtnDisabled}>Submit</button>
            </div>
        </div>
    )
}

const CheckBoxElem = ({ Title, Options, Selected = [], OnChange, OnChangeAll, dataColumn, mainKey }) => {
    let opt = Options.map(o => {
        return <CheckBox key={`${Title}-${o.value}${mainKey === null ? '' : `-${mainKey}`}`} Options={null} onChange={OnChange} Title={o.displayText} checked={Selected.includes(o.value)} Value={o.value} dataColumn={dataColumn} />
    });
    return (
        <div className="input-item" style={{ width: "auto" }}>
            <label className="label">{Title}</label>
            <CheckBox onChange={OnChangeAll} Title="All" Options={Options} checked={(Selected.length === Options.length && Selected.length > 0)} Value="All" dataColumn={dataColumn} />
            {opt}
        </div>
    )
}

const CheckBox = (props) => {
    const OnChange = (e) => {
        props.onChange(props.Value, props.dataColumn, props.Options);
    }
    return (
        <label className="checkbox checkbox-checked" >
            <input type="checkbox" style={{ width: "0" }} value={props.Value} checked={props.checked} onChange={OnChange} />
            <i className="icon icon-checkbox"></i>
            <span className="text">{props.Title}</span>
        </label>
    )
}


export default Panel;