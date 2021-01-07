import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import Filter from './Filter';
import qs from "qs";

const App = () =>{
    const [state,setstate] = useState(0);
    const [siteList, setSiteList] = useState();
    const [stateFilter, setFilter] = useState({
        'SelectSite': [],
        'Type': 0,
        'User': '',
    });


    useEffect(() => {
        console.log(123)
        let param = {
            Url: 'Common/AdminUserSiteList'
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {

                setSiteList(res.data.data);
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }, []);

    const Filter_OnChange = (v, dataColumn) => {
        stateFilter[dataColumn] = v;
        setFilter({
            ...stateFilter
        });
    }

    const Filter_CheckBox_OnChange = (v, dataColumn) => {
        let isRemove = stateFilter[dataColumn].includes(v);
        let newChecked = isRemove
            ? stateFilter[dataColumn].filter(value => value !== v)
            : stateFilter[dataColumn].push(v);
        let currentTab = stateFilter[dataColumn];
        if (newChecked.length === 0) {
            currentTab = [];
        } else if (currentTab === null) {
            currentTab = newChecked;
        }

        if (isRemove && !newChecked.includes(currentTab)) {
            currentTab = newChecked.length === 0 ? [] : newChecked;
        }
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
        if (v !== "All") return false;
        let currentTab = (stateFilter[dataColumn].length === options.length)
            ? []
            : options.map(o => {
                return o.value
            })
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_Submit = () => {
        if (stateFilter.SelectSite.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'BonusManage/GetReferralCustList',
            Data: qs.stringify({
                SelectSite: stateFilter.SelectSite,
                CurrencyType: stateFilter.Type,
                UserName: stateFilter.User,
                PageNumber: 1
            })
        };

        // WebAPI.PostData(param).then((res) => {
        //     if (res.data.errorCode === 0) {
        //         setDatas(
        //             {
        //                 PageInfo: {
        //                     totalPages: res.data.totalPages,
        //                     pageNumber: 1,
        //                     rowCount: res.data.rowCount
        //                 },
        //                 Datas: [...res.data.data],
        //                 FilterInfo: {
        //                     ...stateFilter
        //                 }
        //             }
        //         )
        //     } else {
        //         alert(res.data.message ?? 'failed to get data.');
        //     }
        // })
    }

    return (
        <div className="container-fluid">
            <h4 className="page-name">Referral Report</h4>
            <Filter filterInfo={stateFilter} onChange={Filter_OnChange} onChangeCheckBox={Filter_CheckBox_OnChange} onChangeAllCheckBox={Filter_CheckBox_CheckAll} ClickSubmit={Filter_Submit} SiteList={siteList} />
            {/* {stateDatas.Datas === null ? null : <Result {...stateDatas} ChangePage={Result_ChangePage} ExportToExcel={Result_ExportToExcel} />} */}
        </div>
    );
}

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}