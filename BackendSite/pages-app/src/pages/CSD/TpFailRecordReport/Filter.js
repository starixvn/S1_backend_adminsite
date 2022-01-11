import React from "react";
import moment from 'moment';
import DateTimePicker from "react-datetime";

const defTpList = [
    { value:7,  displayText:"YL" }
  ];

const Panel = (props) => {
    let siteListOptions = [];
    if (props.SiteList) {
        siteListOptions = props.SiteList.map(i => {
            return {
                value: Number(i.siteId),
                displayText: i.siteName
            };
        });
    }
    return (
        <div className="filter">
            <div className="row-flex">
                <DropDownBox 
                    columnTitle="Site"
                    options={siteListOptions}
                    hasAll={true}
                    selectedValue={0}
                    dataColumn="SiteId"
                    onChange={props.onChange}
                />
                <DropDownBox 
                    columnTitle="Vendor"
                    options={defTpList}
                    hasAll={true}
                    selectedValue={0}
                    dataColumn="TpId"
                    onChange={props.onChange}
                />
                <DateTimePickerElem
                    columnTitle="Trans Time From"
                    dateTimePickerProps={{
                        defaultValue: props.filterInfo.StartTime,
                        timeFormat: false,
                        onBlur: e => {
                            try {
                                props.onChange(
                                    moment(e).format('l'),
                                    "StartTime"
                                );
                            } catch (ex) { console.log(ex); }
                        }
                    }}
                />
                <DateTimePickerElem
                    columnTitle="To (Adjusted to GMT+8)"
                    dateTimePickerProps={{
                        defaultValue: props.filterInfo.EndTime,
                        timeFormat: false,
                        onBlur: e => {
                            try {
                                props.onChange(
                                    moment(e).format('l'),
                                    "EndTime"
                                );
                            } catch (ex) { console.log(ex); }
                        }
                    }}
                />
                <div className="button-group">
                    <button className="btn btn-search" type="submit" onClick={props.SubmitBtnDisabled ? null : props.ClickSubmit} disabled={props.SubmitBtnDisabled}>Submit</button>
                </div>
            </div>
            <div className="row-flex">
                <InputField
                    columnTitle="User"
                    defaultValue=""
                    dataColumn="UserName"
                    onChange={props.onChange}
                />
                <InputField
                    columnTitle="Trans ID"
                    defaultValue=""
                    dataColumn="TransId"
                    onChange={props.onChange}
                />
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

const DateTimePickerElem = (props) => {
    return (
        <div className="input-item">
            <span className="label">{props.columnTitle}</span>
            <DateTimePicker {...props.dateTimePickerProps} />
        </div>
    );
};

const DropDownBox = (props) => {
    const onChange = (e) => {
        if(props.onChange){
            props.onChange(e.target.value, props.dataColumn);
        }
    }
    let optionRows = props.options.map(item => {
        return(
            <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
                {item.displayText}
            </option>
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