import React from "react";
import InputBox from "./Components/InputBox";
import DateTimePickerElem from "./Components/DateTimePickerElem"
import SiteCheckList from "./Components/SiteCheckList"
import DropDownBox from "./Components/DropDownBox"

if (process.env.NODE_ENV !== "production") {
    window.gCurrencyList = [2, 4, 13, 15, 51, 61];
    window.i18n = {
        lbl_Currency2: "MYR",
        lbl_Currency4: "THB",
        lbl_Currency13: "RMB",
        lbl_Currency15: "IDR",
        lbl_Currency51: "VND",
        lbl_Currency61: "INR",
    };
}
const defDateType = {
    0: "",
    1: "Today",
    2: "Yesterday",
    3: "This Week",
    4: "Last Week",
    5: "This Month",
    6: "Last Month",
};

export default function Filter ({ siteList, filterInfo, onChange = f => f, siteChecked = g => g, search = h => h, datePickerOnChange = i => i }) {
    return (
        <div className="filter">
            <div className="row-flex">
                <SiteCheckList siteList={siteList} siteChecked={siteChecked} />
                <InputBox title="UserName" onChange={onChange} dataColumn="userName" />
                <DropDownBox
                    columnTitle="Currency"
                    options={window.gCurrencyList.map((curr) => {
                        return { displayText: window.i18n["lbl_Currency" + curr], value: curr };
                    })}
                    hasAll={true}
                    selectedValue={0}
                    onChange={onChange}
                    dataColumn="currency"/>
                <DateTimePickerElem
					title="From Date"
                    datetime={filterInfo.startDate}
                    timeFormat={false}
                    onChange={onChange}
                    dataColumn="startDate"/>
                <DateTimePickerElem
					title="To Date"
                    datetime={filterInfo.endDate}
                    timeFormat={false}
                    onChange={onChange}
                    dataColumn="endDate"/>
                <DropDownBox
                    columnTitle="Date Quick Picker"
                    options={Object.keys(defDateType).map((key) => {
                        return { displayText: defDateType[key], value: key };
                    })}
                    hasAll={false}
                    onChange={datePickerOnChange}
                    selectedValue={filterInfo.dateQuickPicker}
                    dataColumn="dateQuickPicker" />
            </div>
            <button className="btn btn-add" 
                    style={{ marginTop: "20px", marginBottom: "40px" }} 
                    onClick={search}> Search
            </button>
        </div>
    )
}