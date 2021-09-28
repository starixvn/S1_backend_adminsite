import React, { useState, useEffect, useReducer } from "react";
import InputBox from "./Components/InputBox";
import SiteRadioGroup from "./Components/RadioGroup";
import TpCategoryCheckList from "./Components/TpCategoryCheckList"
import SubPanel from "./SubPanel";
import WebAPI from "../../../module//WebAPI";

function reducer (state, action) {
    switch (action.type) {
        case 'initial' :
            return action.data.data;
        case "categoryCheck": 
            let idx = state.findIndex(x => x.categoryId === action.data.id);
            state[idx].checked = !action.data.checked
            state[idx].gameTypeBetLimit = action.data.gameTypeBetLimit
            return [...state];
        case "inputBetLimit":
            const [limit, categoryId] = action.data.id.split(/[\s-]+/);
            let idxx = state.findIndex(x => x.categoryId === parseInt(categoryId));
            if(limit == "MinBet") {
                state[idxx].minBet = action.data.value;
            } else if(limit == "MaxBet") {
                state[idxx].maxBet = action.data.value;
            }
            return [...state];
        case "gameTypeCheck":
            const [cateId, gameTypeId] = action.data.id.split(/-(.+)/)

            state.forEach(a => {
                if(a.categoryId === parseInt(cateId)) {
                    a.gameTypeBetLimit.forEach(b => {
                        if(b.typeId === parseInt(gameTypeId))
                            b.checked = !action.data.checked
                        })    
                }
            });
            return [...state];
        case "inputTpBetLimit":
            const [limit1, categoryId1, gametypeId1] = action.data.id.split(/[\s-]+/);

            state.forEach(a => {
                if(a.categoryId === parseInt(categoryId1)) {
                    a.gameTypeBetLimit.forEach(b => {
                        if(b.typeId === parseInt(gametypeId1)) {
                            if(limit1 == "MinBet") {
                                b.minBet = action.data.value;
                            } else if(limit1 == "MaxBet") {
                                b.maxBet = action.data.value;
                            }
                        }
                    })    
                }
            });
            
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
    const [currentTab, setTab] = useState({ id:0, name:"" });

    useEffect(() => {
        WebAPI.fetchData({ Url: "UserBetLimit/GetTpCategoryList" }).then((res) => {
            if (res.data.errorCode === 0) {
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

    const TpCategoryCheck = (categoryId, categoryName, checked) => {
        if(!checked) {
            WebAPI.fetchData({ Url: `UserBetLimit/GetTpGameTypeListbyCategory/${categoryId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    dispatch({ type: "categoryCheck", data: { id: categoryId, checked: checked, gameTypeBetLimit: res.data.data } });
                    if(currentTab.id === 0)
                    {
                        setTab({id: categoryId,name:categoryName})
                    }
                } else {
                  alert(res.data.message ?? "failed to get data.");
                }
              });
        }
        else
        {
            dispatch({ type: "categoryCheck", data: { id: categoryId, checked: checked, gameTypeBetLimit: []} });
        }
    }
    
    const ClickTab = (categoryId, categoryName) => {
        setTab({ id: categoryId, name: categoryName })
    }

    const SetBetLimit = (value, inputId) => {
        dispatch({ type: "inputBetLimit", data: { id: inputId, value: value } });
    }

    const EnableGameType = (id, checked) => {
        dispatch({ type: "gameTypeCheck", data: { id: id, checked: checked} });
    }

    const SetTpBetLimit = (value, inputId) => {
        dispatch({ type: "inputTpBetLimit", data: { id: inputId, value: value } });
    }

    const Submit = () => {
        if (siteId === 0) {
            alert("Please select a site");
            return false;
        }
        if (userName === '') {
            alert("Please enter UserName");
            return false;
        }
        let betSetting = [];
        tpCategories.forEach(a => {
            if(a.checked) {    
                let tpBetSetting = []
                a.gameTypeBetLimit.forEach( b => {
                    if(b.checked) {
                        tpBetSetting.push({ GameTypeId: b.typeId, MinBet: b.minBet, MaxBet: b.maxBet })
                    }
                })

                betSetting.push({ TpCategory:a.categoryId, MinBet:a.minBet, MaxBet:a.maxBet, TpBetSetting: tpBetSetting})
            }
        })
        InsertOrUpdate(userName, siteId, betSetting)
    };

    return (
        <>
            <h4 className="page-name">Edit</h4>
            <div className="filter noBGcolor"><SiteRadioGroup title="Available Sites" siteList={siteList} selectSite={siteId} disabled={!IsAdd} toggle={setSite}/></div>
            <div className="row-flex"><InputBox title="UserName" value={userName} disabled={!IsAdd} onChange={setUserName} dataColumn = "UserName"/></div>
            <div className="filter noBGcolor"><TpCategoryCheckList title="Available Category" tpCategoryList={tpCategories} toggle={TpCategoryCheck} /></div>
            {currentTab.id !==0 && currentTab.name !=="" ? <SubPanel tpGames={tpCategories} currentTab={currentTab} clickTab={ClickTab} setBetLimit={SetBetLimit} enableGameType={EnableGameType} setTpBetLimit={SetTpBetLimit} />:null}
            <div className="filter float-btn-group">
                <button className="btn btn-back" onClick={ClickBack}>Back</button>
                <button className="btn btn-add" onClick={Submit}> Submit</button>
            </div>
        </>
    )
}

export default Edit