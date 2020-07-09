import React from "react";
import { DashBoardTable } from "./DashBoardTable";

export const DashBoardList = (props) => {
    let list = Object.keys(props.siteDatas).map((siteDatasKey) => {
        return (<div key={`site-${siteDatasKey}`} className="panel mt-20">
            <a className="panel-title" data-toggle="collapse" href={`#collapseExample-${siteDatasKey}`} role="button" aria-expanded="true" aria-controls="collapseExample">
                {props.siteDatas[siteDatasKey].siteName}
            </a>
            <div className="panel-body" >
                <div className="collapse show" id={`collapseExample-${siteDatasKey}`}>
                    <div>
                        <table>
                            <tbody>
                                <tr>
                                    {
                                        Object.keys(props.siteDatas[siteDatasKey]).filter(key => key !== "siteName").sort().map((siteDatasValue) => {
                                            return <DashBoardTable key={`siteTable-${siteDatasKey}-${siteDatasValue}`} tableData={props.siteDatas[siteDatasKey][siteDatasValue]} />
                                        })
                                    }
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        )
    });
    return <>{list}</>;
}