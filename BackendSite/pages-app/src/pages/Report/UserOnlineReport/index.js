import React, { useState, useRef, useEffect } from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';

const App = () => {
    const [stateFilter, setFilter] = useState({
        'SiteIds': [],
        'SelectDate': moment().format('l')
    });

    const [stateDatas, setDatas] = useState({
        Datas: null,
        FilterInfo: null,
        SiteList: null,
        MaxCount: null
    });

    const [siteList, setSiteList] = useState();
    const [SubmitStatus, setSubmitStatus] = useState(false);

    useEffect(() => {
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
        setFilter({
            ...stateFilter,
            [dataColumn]: v
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
        if (stateFilter.SiteIds.length === 0) {
            alert("Please Choose at least one Site.")
            return false;
        }
        if (!moment(stateFilter.SelectDate).unix()) {
            alert("Please Choose Select Date of Report.")
            return false;
        }
        let param = {
            Url: 'Report/GetUserOnlineReport',
            Data: qs.stringify({
                SiteIds: stateFilter.SiteIds,
                SelectDate: moment(stateFilter.SelectDate).format('YYYY-MM-DD')
            })
        };

        setSubmitStatus(true);

        WebAPI.PostData(param).then((res) => {

            setSubmitStatus(false);
                
            if (res.data.errorCode === 0) {

                let result = [];

                res.data.data.forEach(elem => {

                    let getItem = result.find(x=> x.localTime === elem.localTime);
                    let onlineCountColDef = `online_${elem.siteId}`;

                    if(getItem){
                        getItem.onlineSummary += elem.maxCount;

                        getItem[onlineCountColDef] = elem.maxCount;
                    }
                    else
                    {
                        result.push({
                            'localTime':elem.localTime,

                            'onlineSummary':elem.maxCount,
                            [onlineCountColDef]:elem.maxCount,                          
                        });
                    }

                });

                let GetMaxCount=Math.max.apply(Math, result.map(function(o) { return o.onlineSummary; }));

                setDatas(
                    {
                        Datas: result,
                        FilterInfo: {
                            ...stateFilter
                        },
                        SiteList: siteList,
                        MaxCount: GetMaxCount
                    }
                )
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    return (
        <div className="container-fluid">
            <h4 className="page-name">User Online Report</h4>
            <Filter filterInfo={stateFilter} 
            onChange={Filter_OnChange} 
            onChangeCheckBox={Filter_CheckBox_OnChange} 
            onChangeAllCheckBox={Filter_CheckBox_CheckAll} 
            ClickSubmit={Filter_Submit} 
            SiteList={siteList}
            SubmitBtnDisabled={SubmitStatus} />
            {stateDatas.Datas === null 
            ? null 
            : <Result {...stateDatas}/>}
        </div>
    );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}