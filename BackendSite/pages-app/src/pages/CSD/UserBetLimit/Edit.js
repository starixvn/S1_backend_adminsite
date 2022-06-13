import React, { useState, useEffect, useReducer } from "react";
import CustSearchPanel from "./Components/CustSearchPanel";
import CustInfoPanel from "./Components/CustInfoPanel";
import TpCategoryCheckList from "./Components/TpCategoryCheckList"
import SubPanel from "./SubPanel";
import WebAPI from "../../../module//WebAPI";

function reducer(state, action) {
    switch (action.type) {
        case 'initial':
            return action.data.data;
        case "categoryCheck":
            let idx = state.findIndex(x => x.categoryId === action.data.id);
            state[idx].checked = !action.data.checked
            state[idx].gameTypeBetLimit = action.data.gameTypeBetLimit
            return [...state];
        case "gameTypeCheck":
            const [cateId, gameTypeId] = action.data.id.split(/-(.+)/)

            state.forEach(a => {
                if (a.categoryId === parseInt(cateId)) {
                    a.gameTypeBetLimit.forEach(b => {
                        if (b.typeId === parseInt(gameTypeId))
                            b.checked = !action.data.checked
                    })
                }
            });
            return [...state];
        case "groupChange":
            const [col, cacId, gtId] = action.data.id.split(/[\s-]+/);
            console.log(col)
            console.log(cacId)
            console.log(gtId)

            state.forEach(a => {
                if (a.categoryId === parseInt(cacId)) {
                    a.gameTypeBetLimit.forEach(b => {
                        if (b.typeId === parseInt(gtId)) {
                            b.selectedValue = action.data.value;
                        }
                    })
                }
            });
            return [...state];
        default:
            return [...state];
    }
}

function Edit({ IsAdd, CustId, ClickBack = f => f, InsertOrUpdate = g => g }) {

    const [tpCategories, dispatch] = useReducer(reducer, []);
    const [siteList, setSiteList] = useState([]);
    const [searchState, setSearch] = useState({ siteId: 1, userName: "" });
    const [currentTab, setTab] = useState({ id: 0, name: "" });
    const [customer, setCustomer] = useState();

    useEffect(() => {

        WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data);
            }
            else {
                alert(res.data.message ?? "failed to get data.");
            }
        });

        if (!IsAdd && CustId != 0) {
            WebAPI.fetchData({ Url: `UserBetLimit/GetByCustId/${CustId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    let temp = res.data.data;
                    console.log(temp)
                    setCustomer({
                        siteId: res.data.data["siteId"],
                        siteName: res.data.data["siteName"],
                        custId: res.data.data["custId"],
                        userName: res.data.data["userName"],
                        currencyId: res.data.data["currencyId"]
                    })
                    dispatch({ type: "initial", data: { data: temp.betSetting } });
                    let betSetting =  res.data.data["betSetting"];
                    setTab({ id: betSetting[0].categoryId, name: betSetting[0].categoryName })
                }
                else {
                    alert(res.data.message ?? "failed to get data.");
                }
            });
        } else {
            WebAPI.fetchData({ Url: "UserBetLimit/GetTpCategoryList" }).then((res) => {
                if (res.data.errorCode === 0) {
                    dispatch({ type: "initial", data: { data: res.data.data } });
                }
                else {
                    alert(res.data.message ?? "failed to get data.");
                }
            });
        }

    }, []);

    const TpCategoryCheck = (categoryId, categoryName, checked) => {
        if (!checked) {
            WebAPI.fetchData({ Url: `UserBetLimit/GetTpBetLimitGroup?categoryId=${categoryId}&currencyId=${customer.currencyId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    dispatch({ type: "categoryCheck", data: { id: categoryId, checked: checked, gameTypeBetLimit: res.data.data } });
                    if (currentTab.id === 0) {
                        setTab({ id: categoryId, name: categoryName })
                    }
                } else {
                    alert(res.data.message ?? "failed to get data.");
                }
            });
        }
        else {
            if (categoryId == currentTab.id) {
                let first = tpCategories.filter(a => a.checked === true && a.categoryId !== categoryId);
                if (first.length === 0)
                    setTab({ id: 0, name: "" })
                else
                    setTab({ id: first[0].categoryId, name: first[0].categoryName })
            }
            dispatch({ type: "categoryCheck", data: { id: categoryId, checked: checked, gameTypeBetLimit: [] } });
        }
    }

    const clickTab = (categoryId, categoryName) => {
        setTab({ id: categoryId, name: categoryName })
    }

    const enableGameType = (id, checked) => {
        dispatch({ type: "gameTypeCheck", data: { id: id, checked: checked } });
    }

    const groupChange = (value, title) => {
        dispatch({ type: "groupChange", data: { id: title, value: value } });
    }

    const selectSite = (siteId) => {
        setSearch({ ...searchState, siteId: siteId })
    }

    const inputUserName = (userName) => {
        setSearch({ ...searchState, userName: userName })
    }

    const searchCustomer = () => {
        let param = {
            Url: "CustManage/GetCustInfoDetails",
            Data: {
                UserName: searchState.userName,
                SiteId: searchState.siteId
            }
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setCustomer({
                    siteId: res.data.data["siteId"],
                    siteName: res.data.data["siteName"],
                    custId: res.data.data["custId"],
                    userName: res.data.data["userName"],
                    currencyId: res.data.data["currencyId"]
                })
                console.log(res.data.data)
            }
            else {
                alert(res.data.message);
            }
        });
    }

    const Submit = () => {
        if(customer === null) {
            alert("Please input a customer");
            return false;
        }
        if (tpCategories.filter(a => a.checked === true).length === 0) {
            alert("Please select at least one category.")
            return false;
        }
        let checked = true;
        let betSetting = [];
        tpCategories.forEach(a => {
            if (a.checked) {
                let tpBetSetting = []
                a.gameTypeBetLimit.forEach(b => {
                    if (b.checked) {
                        if (b.minBet === 0) {
                            alert(`${b.typeName} MinBet can't be 0`);
                            checked = false;
                            return;
                        } else if (b.maxBet === 0) {
                            alert(`${b.typeName} MaxBet can't be 0`);
                            checked = false;
                            return;
                        }
                        let range = b.limitGroup.find(x => x.group === b.selectedValue).range;
                        tpBetSetting.push({ GameTypeId: b.typeId, GameTypeName: b.typeName, LimitGroup: b.selectedValue, LimitRange: range })
                    }
                })
                betSetting.push({ TpCategory: a.categoryId, TpCategoryName: a.categoryName, TpBetSetting: tpBetSetting })
            }
        })
        if (checked) {
            InsertOrUpdate(customer, betSetting)
        }
    };

    return (
        <>
            <h4 className="page-name">Edit</h4>
            {IsAdd ? <CustSearchPanel siteList={siteList} inputSearch={searchState} setSite={selectSite} setUserName={inputUserName} search={searchCustomer} /> : null}
            {customer != null ? <CustInfoPanel custData={customer} /> : null}
            {customer != null ? <div className="filter noBGcolor"><TpCategoryCheckList title="Available Category" tpCategoryList={tpCategories} toggle={TpCategoryCheck} /></div> : null}
            {currentTab.id !== 0 && currentTab.name !== "" ? <SubPanel tpGames={tpCategories} currentTab={currentTab} clickTab={clickTab} groupChange={groupChange} enableGameType={enableGameType} /> : null}
            <div className="filter">
                <button className="btn btn-back" onClick={ClickBack}>Back</button>
                <button className="btn btn-add" onClick={Submit}> Submit</button>
            </div>
        </>
    )
}

export default Edit