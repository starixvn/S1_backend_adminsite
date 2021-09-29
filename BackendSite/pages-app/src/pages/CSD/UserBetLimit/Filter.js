import React from "react";
import InputBox from "./Components/InputBox";
import DateTimePickerElem from "./Components/DateTimePickerElem"
import SiteCheckList from "./Components/SiteCheckList"

function Filter ({ siteList, filterInfo, onChange = f => f, siteChecked = g => g, search = h => h }) {
    return (
        <div className="filter">
            <div className="row-flex">
                <SiteCheckList siteList={siteList} siteChecked={siteChecked} />
                <InputBox title="UserName" onChange={onChange} dataColumn="UserName" />
                <DateTimePickerElem
					title="From Date"
                    value={filterInfo.StartDate}
                    timeFormat={false}
                    onChange={onChange}
                    dataColumn="StartDate"/>
                <DateTimePickerElem
					title="To Date"
                    value={filterInfo.EndDate}
                    timeFormat={false}
                    onChange={onChange}
                    dataColumn="EndDate"/>
            </div>
            <button className="btn btn-add" 
                    style={{ marginTop: "20px", marginBottom: "40px" }} 
                    onClick={search}> Search
            </button>
        </div>
    )
}


export default Filter