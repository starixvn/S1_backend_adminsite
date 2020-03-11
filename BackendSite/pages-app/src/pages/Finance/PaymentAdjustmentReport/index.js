import React, {useState, useRef, useEffect} from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';

const App = ()=>{
    const [stateFilter, setFilter] = useState({
        'TransId':'',
        'Currency':0,
        'StartDate':null,
        'EndDate':null,
        'User':'',
        'Type':0,
        'CustId':0
    });

    const [stateRef] = useState({
        f_trigger_Start:useRef(null),
        f_trigger_End:useRef(null),
        selectdate_end:useRef(null),
        selectdate_start:useRef(null),
    });

    const [stateDatas, setDatas] = useState({
        PageInfo:null,
        Datas:null,
        FilterInfo:null
    });

    useEffect(() => {
        let f_trigger_Start = stateRef.f_trigger_Start.current;
        let f_trigger_End = stateRef.f_trigger_End.current;
        let selectdate_end = stateRef.selectdate_end.current;
        let selectdate_start = stateRef.selectdate_start.current;
        
        /*eslint-disable */
        Calendar.setup({
            inputField: selectdate_end, // id of the input field
            ifFormat: "mm/dd/y",        // format of the input field
            button: f_trigger_End,  // trigger for the calendar (button ID)
            singleClick: true              // double-click mode
        });

        Calendar.setup({
            inputField: selectdate_start, // id of the input field
            ifFormat: "mm/dd/y",        // format of the input field
            button: f_trigger_Start,  // trigger for the calendar (button ID)
            singleClick: true              // double-click mode
        });
        /*eslint-enable */     
    }, [stateRef]);

    const Filter_OnChange = (v, dataColumn)=>{
        setFilter({
            ...stateFilter,
            [dataColumn]:v
        });
    }

    const Filter_Submit = ()=>{
        let startDate = moment(stateRef.selectdate_start.current.value).unix();
        let endDate = moment(stateRef.selectdate_end.current.value).add(1, 'days').unix()-1;
        
        let param = {
            Url: 'PaymentAdjustmentReport/GetPaymentAdjustmentReport',
            Data:qs.stringify({
                AdjustType:stateFilter.Type,
                CurrencyId:stateFilter.Currency,
                StartDate:startDate,
                EndDate:endDate,
                UserName:stateFilter.User,
                TransId:stateFilter.TransId,
                PageNumber:1
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas(
                    {
                        PageInfo:{
                            totalPages:res.data.totalPages,
                            pageNumber:1,
                            rowCount:res.data.rowCount
                        },
                        Datas:[...res.data.data],
                        FilterInfo:{
                            ...stateFilter,
                            StartDate:startDate,
                            EndDate:endDate
                        }
                    }
                )
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ChangePage = (PageNumber)=>{
        let param = {
            Url: 'PaymentAdjustmentReport/GetPaymentAdjustmentReport',
            Data:qs.stringify({
                AdjustType:stateDatas.FilterInfo.Type,
                CurrencyId:stateDatas.FilterInfo.Currency,
                StartDate:stateDatas.FilterInfo.StartDate,
                EndDate:stateDatas.FilterInfo.EndDate,
                UserName:stateDatas.FilterInfo.User,
                TransId:stateDatas.FilterInfo.TransId,
                PageNumber:PageNumber
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas({
                    ...stateDatas,
                    PageInfo:{
                        ...stateDatas.PageInfo,
                        pageNumber:PageNumber
                    },
                    Datas:[...res.data.data],
                });
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ExportToExcel = ()=>{
        let param = {
            Url: 'PaymentAdjustmentReport/ExportPaymentAdjustmentReport',
            Data:{
                AdjustType:stateDatas.FilterInfo.Type,
                CurrencyId:stateDatas.FilterInfo.Currency,
                StartDate:stateDatas.FilterInfo.StartDate,
                EndDate:stateDatas.FilterInfo.EndDate,
                UserName:stateDatas.FilterInfo.User,
                TransId:stateDatas.FilterInfo.TransId
            }
        };

        WebAPI.DownloadData(param);
    }

    return(
        <div className="container-fluid">
            <h4 className="page-name">Payment Adjustment</h4>
            <Filter filterInfo={stateFilter} refInfo={stateRef} onChange={Filter_OnChange} ClickSubmit={Filter_Submit}/>
            {stateDatas.Datas === null ? null : <Result {...stateDatas} ChangePage={Result_ChangePage} ExportToExcel={Result_ExportToExcel}/>}
        </div>
    );
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}