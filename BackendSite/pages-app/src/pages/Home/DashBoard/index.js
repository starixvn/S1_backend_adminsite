import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import WebAPI from '../../../module/WebAPI';
import moment from 'moment';
import qs from 'qs';
import { DashBoardList } from "./DashBoardList";

const DashBoardInfo = () => {
    let date = new Date();
    // let startDate = moment(date).add(-1, 'days').startOf('day').unix();
    // let endDate = moment(date).add(-1, 'days').endOf('day').unix();
    let startDate = moment('20200102').add(-1, 'days').startOf('day').unix();
    let endDate = moment('20200401').add(-1, 'days').endOf('day').unix();
    const [dashBoardInfoType, setDashBoardInfoType] = useState([
        {
            Title: "Yesterday Registered Customers",
            Field: "YesterdayRegistered",
            ApiUrl: "Home/RegisteredCustomers",
            StartDate: startDate,
            EndDate: endDate

        },
        {
            Title: "Yesterday Deposit Customers",
            Field: "YesterdayDeposit",
            ApiUrl: "Home/DepositCustomers",
            StartDate: startDate,
            EndDate: endDate
        }

    ]);
    const [site, setSite] = useState({});
    const [siteDatas, setSiteDatas] = useState({});

    useEffect(() => {
        let param = { Url: "Marketing/GetAllSites" };
        WebAPI.fetchData(param)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    setSite(response.data === null ? [] : response.data);
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    useEffect(() => {
        if (Object.keys(site).length !== 0) {
            site.data.map((sitevalue) => {
                dashBoardInfoType.map((dashBoardsvalue) => {
                    let param = {
                        Url: dashBoardsvalue.ApiUrl,
                        Data: qs.stringify({
                            SiteId: sitevalue.siteId,
                            StartDate: startDate,
                            EndDate: endDate
                        })
                    };

                    WebAPI.PostData(param)
                        .then(function (response) {
                            if (response.data.errorCode === 0) {
                                setSiteDatas(prevState => {
                                    return {
                                        ...prevState,
                                        [sitevalue.siteId]: { ...prevState[sitevalue.siteId], siteName: sitevalue.siteName, [dashBoardsvalue.Field]: { title: dashBoardsvalue.Title, data: response.data === null ? 0 : response.data.data } }
                                    }
                                });
                            } else {
                                // alert(response.message);
                            }
                        }).catch(function (error) {
                            alert(error);
                        })
                });
            });
        }
    }, [site]);
    return (
        Object.keys(siteDatas).length !== 0 ? <DashBoardList siteDatas={siteDatas} /> : null
    );
}

const App = () => {
    return (
        <div className="container-fluid">
            <h4 className="page-name">Dash Board</h4>
            <div>
                <DashBoardInfo />
            </div>
        </div>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}