import React from "react"
import SiteRadioGroup from "./SiteRadioGroup";
import InputBox from "./InputBox";

export default function CustSearchPanel ({ siteList, inputSearch, setSite=f=>f, setUserName=g=>g, search=h=>h }) {
    return (
        <div className="filter">
            <div className="filter noBGcolor"><SiteRadioGroup title="Available Sites" siteList={siteList} selectSite={inputSearch.siteId} toggle={setSite} /></div>
            <div className="row-flex"><InputBox title="UserName" value={inputSearch.userName} onChange={setUserName} dataColumn="UserName" /></div>
            <button className="btn btn-add"
                    style={{ marginTop: "20px", marginBottom: "40px" }}
                    onClick={search}> Search
            </button>
        </div>
    )
}