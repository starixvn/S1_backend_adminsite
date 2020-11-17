import React, { useState, useRef, useEffect } from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import SingleCurrencyResult from './SingleCurrencyResult';
import MultiCurrencyResult from './MultiCurrencyResult';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';

const App = () => {
    const [stateFilter, setFilter] = useState({
        'SiteIds': [],
        'Currency': 2,
        'StartDate': moment().add(-10, 'days').format('l'),
        'EndDate': moment().format('l')
    });

    const [stateDatas, setDatas] = useState({
        Datas: null,
        FilterInfo: null
    });

    const [siteList, setSiteList] = useState();

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
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'Report/GetMainFinancialReport',
            Data: qs.stringify({
                SiteIds: stateFilter.SiteIds,
                CurrencyId: stateFilter.Currency,
                StartDate: moment(stateFilter.StartDate).unix(),
                EndDate: moment(stateFilter.EndDate).add(1, 'days').unix() - 1
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas(
                    {
                        Datas: [...res.data.data],
                        FilterInfo: {
                            ...stateFilter
                        }
                    }
                )
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const SingleCurrencyResult_ExportToExcel = () => {
        if (stateDatas.FilterInfo.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        console.log(stateDatas);
        let param = {
            Url: 'Report/ExportMainFinancialReport',
            Data: {
                SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
                CurrencyId: stateDatas.FilterInfo.Currency,
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1
            }
        };

        WebAPI.DownloadData(param);
    }

    const MultiCurrencyResult_ExportToExcel = () => {
        if (stateDatas.FilterInfo.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'Report/ExportMainFinancialReport',
            Data: {
                SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
                CurrencyId: stateDatas.FilterInfo.Currency,
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1
            }
        };

        WebAPI.DownloadData(param);
    }

    if(stateFilter.CurrencyId !== 0)
    {
        return (
            <div className="container-fluid">
                <h4 className="page-name">Main Financial Report</h4>
                <Filter filterInfo={stateFilter} onChange={Filter_OnChange} onChangeCheckBox={Filter_CheckBox_OnChange} onChangeAllCheckBox={Filter_CheckBox_CheckAll} ClickSubmit={Filter_Submit} SiteList={siteList} />
                {stateDatas.Datas === null ? null : <SingleCurrencyResult {...stateDatas} ExportToExcel={SingleCurrencyResult_ExportToExcel} />}
            </div>
        );
    }
    else
    {
        return (
            <div className="container-fluid">
                <h4 className="page-name">Main Financial Report</h4>
                <Filter filterInfo={stateFilter} onChange={Filter_OnChange} onChangeCheckBox={Filter_CheckBox_OnChange} onChangeAllCheckBox={Filter_CheckBox_CheckAll} ClickSubmit={Filter_Submit} SiteList={siteList} />
                {stateDatas.Datas === null ? null : <MultiCurrencyResult {...stateDatas} ExportToExcel={MultiCurrencyResult_ExportToExcel} />}
            </div>
        );
    }
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}