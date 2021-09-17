import React, { useState, useEffect, useReducer } from "react";
import TpCategoryBetLimitList from "./Components/TpCategoryBetLimitList"
import InputBox from "./Components/InputBox";
import SiteRadioGroup from "./Components/RadioGroup";
import WebAPI from "../../../module//WebAPI";

function reducer (state, action) {
    switch (action.type) {
        case 'initial' :
            return action.data.data;
        case "categoryCheck": 
            var [categoryId] = action.data.id.split(/[\s-]+/).reverse();
            let idx = state.findIndex(x => x.categoryId === parseInt(categoryId));
            state[idx].checked = !action.data.checked
            return [...state];
        case "inputBetLimit":
            var [limit, categoryId] = action.data.id.split(/[\s-]+/);
            let idxx = state.findIndex(x => x.categoryId === parseInt(categoryId));
            if(limit == "MinBet") {
                state[idxx].minBet = action.data.value;
            } else if(limit == "MaxBet") {
                state[idxx].maxBet = action.data.value;
            }
            return [...state];
        default:
            return [...state];
    }
}

function Edit ({ IsAdd, CustId, ClickBack = f => f, InsertOrUpdate = g => g }) {

    const [tpCategories, dispatch] = useReducer(reducer, []);
    const [siteList, setSiteList] = useState([]);
    const [siteId, setSite] = useState(0);
    const [userName, setUserName] = useState("");

    useEffect(() => {
        WebAPI.fetchData({ Url: "UserBetLimit/GetTpCategoryList" }).then((res) => {
            if (res.data.errorCode === 0) {
                console.log(res.data.data)
                dispatch({ type: "initial", data: { data: res.data.data}  });
            } 
            else {
                alert(res.data.message ?? "failed to get data.");
            }
        });

        WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data);
            } 
            else {
                alert(res.data.message ?? "failed to get data.");
            }
        });

    }, []);

    const SwitchTpCategory = (switchId, checked) => {
        dispatch({ type: "categoryCheck", data: { id: switchId, checked: checked } });
    }

    const InputBetLimit = (value, inputId) => {
        dispatch({ type: "inputBetLimit", data: { id: inputId, value: value } });
    }

    const Submit = () => {
        if (siteId === 0) {
            alert("Please select a site");
            return false;
        }
        // if (userName === '') {
        //     alert("Please enter UserName");
        //     return false;
        // }
        let betSetting = [];
        tpCategories.forEach(a => {
            if(a.checked) {    
                betSetting.push({ TpCategory:a.categoryId, MinBet:a.minBet, MaxBet:a.maxBet })
            }
        })
        console.log(betSetting)
        //InsertOrUpdate(userName, siteId, betSetting)
    };

    return (
        <>
            <h4 className="page-name">Edit</h4>
            <div className="filter noBGcolor"><SiteRadioGroup title="Available Sites" siteList={siteList} selectSite={siteId} disabled={!IsAdd} toggle={setSite}/></div>
            <div className="row-flex"><InputBox title="UserName" value={userName} disabled={!IsAdd} onChange={setUserName} dataColumn = "UserName"/></div>
            <div className="filter"><TpCategoryBetLimitList categories={tpCategories} toggle={SwitchTpCategory} input={InputBetLimit}/></div>
            <div className="filter float-btn-group">
                <button className="btn btn-back" onClick={ClickBack}>Back</button>
                <button className="btn btn-add" onClick={Submit}> Submit</button>
            </div>
        </>
    )
}

export default Edit